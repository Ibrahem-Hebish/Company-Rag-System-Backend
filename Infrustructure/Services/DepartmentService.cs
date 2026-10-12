namespace Infrustructure.Services;

public sealed class DepartmentService(
    AppDbContext dbContext,
    ITenantResolver tenantResolver,
    IDocumentChannel documentChannel) : ResponseHandler, IDepartmentService
{
    public async Task<Response<string>> CreateAsync(CreateDepartmentRequest request, CancellationToken cts = default)
    {
        var companyId = tenantResolver.GetCompanyId();

        var company = await dbContext.Companies.FirstOrDefaultAsync(c => c.Id == companyId, cts);

        if (company is null)
            return NotFound<string>("Company not found.");

        var department = Department.Create(request.Name, companyId);

        await dbContext.Departments.AddAsync(department, cts);

        await dbContext.SaveChangesAsync(cts);

        return Created("Department created successfully.");
    }

    public async Task<Response<string>> DeleteAsync(string id, CancellationToken cts = default)
    {
        try
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync(cts);

            var companyId = tenantResolver.GetCompanyId();

            var department = await dbContext.Departments
                                            .Include(d => d.Employees)
                                            .Include(d => d.Documents)
                                            .FirstOrDefaultAsync(d => d.Id == id, cts);

            if (department is null)
                return NotFound<string>("Department not found.");

            var companyManager = department.Employees.FirstOrDefault(e => e.Role == UserRole.CompanyManager);
            var departmentManager = department.Employees.FirstOrDefault(e => e.Role == UserRole.DepartmentManager);

            if(departmentManager is not null)
                department.UnsetManager();

            if(companyManager is not null)
            {
                var company = await dbContext.Companies.FindAsync(companyId, cts);

                company?.UnSetManager();

                var managerEmployees = await dbContext.Employees.IgnoreQueryFilters()
                                                          .Where(e => e.CompanyId == companyId && e.ManagerId == companyManager.Id)
                                                          .ToListAsync(cts);

                foreach(var employee in managerEmployees)
                    employee.UnSetManager();
            }

            var employeesIds = department.Employees.Select(e => e.Id);

            var users = await dbContext.Users
                .Where(u => employeesIds.Contains(u.Id))
                .ToListAsync(cts);

            var documents = await dbContext.Documents
                                                     .IgnoreQueryFilters()
                                                     .Where(d => employeesIds.Contains(d.UploaderId))
                                                     .ToListAsync(cts);

            foreach (var document in documents)
                document.UnsetUploader();

            foreach(var employee in department.Employees)
                employee.UnSetManager();

            await dbContext.SaveChangesAsync(cts);

            dbContext.Users.RemoveRange(users);
            dbContext.Documents.RemoveRange(department.Documents);
            dbContext.Departments.Remove(department);

            await dbContext.SaveChangesAsync(cts);

            await transaction.CommitAsync(cts);

            var @event = new DocumentsDepartmentIsDeletedEvent(
                department.Id,
                [.. department.Documents.Select(d => d.Path)],
                [.. department.Documents.Select(d => d.Id)]);
           
            await documentChannel.WriteDocumnetsDepartmentIsDeletedAsync(@event, cts);

            return Success("Department deleted successfully.");
        }catch(Exception ex)
        {
            await dbContext.Database.RollbackTransactionAsync(cts);

            return InternalServerError<string>($"An error occurred while deleting the department: {ex.Message}");
        }

    }

    public async Task<Response<List<DepartmentResponse>>> GetAllAsync(CancellationToken cts = default)
    {
        var companyId = tenantResolver.GetCompanyId();

        var departments = await dbContext.Departments
                                         .Include(d => d.Manager)
                                         .AsNoTracking()
                                         .Select(d => DepartmentMappings.ToResponse(d))
                                         .ToListAsync(cts);

        if (departments is null || departments.Count == 0)
            return NotFound<List<DepartmentResponse>>("No departments found for the owner.");

        return Success(departments);
    }

    public async Task<Response<DepartmentResponse>> GetByIdAsync(string id, CancellationToken cts = default)
    {
        var companyId = tenantResolver.GetCompanyId();

        var department = await dbContext.Departments
                                        .AsNoTracking()
                                        .Include(d => d.Manager)
                                        .FirstOrDefaultAsync(d => d.Id == id, cts);

        if (department is null)
            return NotFound<DepartmentResponse>("Department not found.");

        return Success(department.ToResponse());
    }

    public async Task<Response<string>> UpdateAsync(UpdateDepartmentRequest request, string id, CancellationToken cts = default)
    {
        var department = await dbContext.Departments
                                         .FirstOrDefaultAsync(d => d.Id == id, cts);

        if (department is null)
            return NotFound<string>("Department not found.");

        department.Update(request.Name);

        await dbContext.SaveChangesAsync(cts);

        return Success("Department updated successfully.");
    }

    public async Task<Response<EmployeeResponse>> GetManagerAsync(CancellationToken cts = default)
    {
        var companyId = tenantResolver.GetCompanyId();

        var departmentId = tenantResolver.GetDepartmentId();

        var manager = await dbContext.Employees
                                    .AsNoTracking()
                                    .Include(e => e.Department)
                                    .Include(e => e.Manager)
                                    .FirstOrDefaultAsync(
           e => e.DepartmentId == departmentId && e.Role == UserRole.DepartmentManager, cts);

        if (manager is null || manager.CompanyId != companyId)
            return NotFound<EmployeeResponse>("There is no employee with department manager role in this department.");

        return Success(manager.ToResponse());
    }
    public async Task<Response<string>> SetManagerAsync(string id, string managerId,  string callerId, string callerRole,CancellationToken cts = default)
    {
        var department = await dbContext.Departments
            .Include(d => d.Company)
            .FirstOrDefaultAsync(d => d.Id == id, cts);

        if (department is null)
            return NotFound<string>("there is no department with this id.");

        var departmentManager = await dbContext.Employees.FindAsync(managerId, cts);

        if (departmentManager is null)
            return NotFound<string>("There is no employee with this id.");

        bool canSetManager = department.CanSetManager(callerId, callerRole);

        if(!canSetManager)
            return Forbidden<string>("You are not authorize to do this action.");

        var isValidManager = department.IsValidManager(departmentManager);

        if (!isValidManager)
            return BadRequest<string>("This manager can not manage this department.");

        department.SetManager(managerId);

        await dbContext.SaveChangesAsync(cts);

        return Success("Department manager is set successfully.");
    }
    public async Task<Response<string>> UnsetManagerAsync(string id, string callerId, string callerRole, CancellationToken cts = default)
    {
        var department = await dbContext.Departments
            .Include(d => d.Company)
            .FirstOrDefaultAsync(d => d.Id == id, cts);

        if (department is null)
            return NotFound<string>("there is no department with this id.");

        if (department.ManagerId is null)
            return BadRequest<string>("This department ha not already had a manager.");

        bool canUnsetManager = department.CanUnsetManager(callerId, callerRole);

        if (!canUnsetManager)
            return Forbidden<string>("You are not authorize to do this action.");

        department.UnsetManager();

        await dbContext.SaveChangesAsync(cts);

        return Success("Department manager is set successfully.");
    }
}
