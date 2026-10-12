using ModelContextProtocol.Protocol;

namespace Infrustructure.Services;

public sealed class EmployeeService(
    AppDbContext dbContext,
    ITenantResolver tenantResolver,
    IPasswordHashingHandler passwordHashingHandler)
    
    : ResponseHandler, IEmployeeService
{
    public async Task<Response<string>> CreateAsync(CreateEmployeeRequest request, CancellationToken cts = default)
    {
        var departmentId = tenantResolver.GetDepartmentId();
        var companyId = tenantResolver.GetCompanyId();

        var department = await dbContext.Departments
                                        .Include(d => d.Company)
                                        .FirstOrDefaultAsync(d => d.Id == departmentId, cts);

        if (department is null)
            return NotFound<string>("Department is not exisit;");

        var exsitingUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email,
            cts);

        if (exsitingUser is not null)
            return BadRequest<string>("Email already exists.");

        var employeeRole = EmployeeRoleConversion.ConvertToUserRole(request.Role);

        if(employeeRole == UserRole.CompanyManager)
        {
            var IsThereCompanyManager = await dbContext.Employees.AnyAsync(
                e => e.CompanyId == companyId && e.Role == UserRole.CompanyManager);

            if (IsThereCompanyManager)
                return BadRequest<string>("Can not save a company manager employee due to this company has a manager.");


        }
        else if(employeeRole == UserRole.DepartmentManager)
        {
            var IsThereDepartmentManager = await dbContext.Employees.AnyAsync(
                e => e.DepartmentId == department.Id && e.Role == UserRole.DepartmentManager);

            if (IsThereDepartmentManager)
                return BadRequest<string>("Can not save a department manager employee due to this department has a manager.");
        }

        if (request.ManagerId is not null)
        {
            var manager = await dbContext.Employees.FindAsync(request.ManagerId, cts);

            if (manager is null)
                return NotFound<string>("Manager is not found.");

            if (employeeRole == UserRole.Employee 
                && (manager.DepartmentId != departmentId || manager.Role != UserRole.DepartmentManager))
                return BadRequest<string>("Manager must be in the same department.");

            else if (employeeRole == UserRole.DepartmentManager 
                && (manager.Role != UserRole.CompanyManager || manager.CompanyId != companyId))
                return BadRequest<string>("Manager must be in the same company.");

            else if (employeeRole == UserRole.CompanyManager)
                return BadRequest<string>("Company Manager can not have manager.");
        }


        var hashedPassword = passwordHashingHandler.HashPasswordBCrypt(request.Password);

        var employee = Employee.Create(
            request.FirstName,
            request.LastName,
            employeeRole,
            request.Email,
            hashedPassword,
            request.PhoneNumber,
            request.Address,
            departmentId,
            companyId,
            request.ManagerId);

        await dbContext.Employees.AddAsync(employee, cts);

        if (request.Role == EmployeeRole.CompanyManager)
            department.Company.SetManager(employee.Id);

        else if (request.Role == EmployeeRole.DepartmentManager)
            department.SetManager(employee.Id);

        await dbContext.SaveChangesAsync(cts);

        return Created("Employee created successfully.");
    }

    public async Task<Response<string>> DeleteAsync(string employeeId, string callerRole, CancellationToken cts = default)
    {
        using var transaction = await dbContext.Database.BeginTransactionAsync(cts);

        try
        {
            var departmentId = tenantResolver.GetDepartmentId();
            var companyId = tenantResolver.GetCompanyId();

            var employee = await dbContext.Employees.FirstOrDefaultAsync(
                e => e.Id == employeeId && e.DepartmentId == departmentId, cts);

            if (employee is null)
                return NotFound<string>("Employee not found.");

            if (employee.Role == UserRole.CompanyManager
                && callerRole != UserRole.Owner.ToString())
                return Forbidden<string>();

            if (employee.Role == UserRole.DepartmentManager
                && (callerRole == UserRole.DepartmentManager.ToString() || callerRole == UserRole.Employee.ToString()))
                return Forbidden<string>();

            if (employee.Role == UserRole.CompanyManager)
            {
                var company = await dbContext.Companies.FindAsync(companyId, cts);

                company?.UnSetManager();
            }
            else if (employee.Role == UserRole.DepartmentManager)
            {
                var department = await dbContext.Departments.FindAsync(departmentId, cts);

                department?.UnsetManager();
            }

            var documents = await dbContext.Documents.Where(
                d => d.UploaderId == employee.Id || d.ModifierId == employee.Id)
                .ToListAsync(cts);

            foreach(var document in documents)
                document.UnsetUploader();

            dbContext.Employees.Remove(employee);

            await dbContext.SaveChangesAsync(cts);

            await transaction.CommitAsync(cts);

            return Deleted<string>("Employee deleted successfully.");
        }catch(Exception ex)
        {
            await transaction.RollbackAsync(cts);

            throw new Exception(ex.Message);
        }
        
    }

    public async Task<Response<List<EmployeeResponse>>> GetAllByCompanyIdAsync(CancellationToken cts = default)
    {
        var companyId = tenantResolver.GetCompanyId();

        var employees = await dbContext.Employees
                                 .AsNoTracking()
                                 .Where(e => e.CompanyId == companyId)
                                 .Select(e => new EmployeeResponse(
                                     e.Id,
                                     e.FirstName,
                                     e.LastName,
                                     e.Email,
                                     e.PhoneNumber,
                                     e.Role.ToString(),
                                     e.Address,
                                     e.DepartmentId,
                                     e.Department.Name,
                                     e.Manager != null ? $"{e.Manager.GetFullName()}" : null,
                                     e.CreatedAt
                                 ))
                                 .ToListAsync(cts);

        if (employees is null || employees.Count == 0)
            return NotFound<List<EmployeeResponse>>("No employees found for the specified company.");

        return Success(employees);
    }

    public async Task<Response<List<EmployeeResponse>>> GetAllByDepartmentIdAsync(CancellationToken cts = default)
    {
        var departmentId = tenantResolver.GetDepartmentId();

        var employees = await dbContext.Employees
                                 .AsNoTracking()
                                 .Where(e => e.DepartmentId == departmentId)
                                 .Select(e => new EmployeeResponse(
                                     e.Id,
                                     e.FirstName,
                                     e.LastName,
                                     e.Email,
                                     e.PhoneNumber,
                                     e.Role.ToString(),
                                     e.Address,
                                     e.DepartmentId,
                                     e.Department.Name,
                                     e.Manager != null ? $"{e.Manager.GetFullName()}" : null,
                                     e.CreatedAt
                                 ))
                                 .ToListAsync(cts);

        if (employees is null || employees.Count == 0)
            return NotFound<List<EmployeeResponse>>("No employees found for the specified department.");

        return Success(employees);
    }

    public async Task<Response<List<EmployeeResponse>>> GetAllByManagerIdAsync(string managerId, CancellationToken cts = default)
    {
        var departmentId = tenantResolver.GetDepartmentId();

        var employees = await dbContext.Employees
                                 .AsNoTracking()
                                 .Where(e => e.DepartmentId == departmentId && e.ManagerId == managerId)
                                 .Select(e => new EmployeeResponse(
                                     e.Id,
                                     e.FirstName,
                                     e.LastName,
                                     e.Email,
                                     e.PhoneNumber,
                                     e.Role.ToString(),
                                     e.Address,
                                     e.DepartmentId,
                                     e.Department.Name,
                                     e.Manager != null ? $"{e.Manager.GetFullName()}" : null,
                                     e.CreatedAt
                                 ))
                                 .ToListAsync(cts);

        if (employees is null || employees.Count == 0)
            return NotFound<List<EmployeeResponse>>("No employees found for the specified manager.");

        return Success(employees);
    }

    public async Task<Response<EmployeeResponse>> GetByIdAsync(string employeeId, CancellationToken cts = default)
    {
        var companyId = tenantResolver.GetCompanyId();
        var departmentId = tenantResolver.GetDepartmentId();

        var employee = await dbContext.Employees
            .Include(e => e.Department)
            .Include(e => e.Manager)
            .FirstOrDefaultAsync(e => e.Id == employeeId && e.DepartmentId == departmentId, cts);

        if (employee is null)
            return NotFound<EmployeeResponse>("Employee not found.");

        return Success(EmployeeMappings.ToResponse(employee));
    }

    public async Task<Response<string>> UpdateAsync(UpdateEmployeeRequest request, string employeeId, CancellationToken cts = default)
    {
        var departmentId = tenantResolver.GetDepartmentId();

        var employee = await dbContext.Employees.FirstOrDefaultAsync(
            e => e.Id == employeeId && e.DepartmentId == departmentId, cts);

        if (employee is null)
            return NotFound<string>("Employee not found.");

        employee.Update(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            request.Address
        );

        await dbContext.SaveChangesAsync(cts);

        return Success("Employee updated successfully.");
    }

    public async Task<Response<string>> SetManagerAsync(SetEmployeeManagerRequest request, string callerId, string callerRole, CancellationToken cts = default)
    {
        var companyId = tenantResolver.GetCompanyId();
        var departmentId = tenantResolver.GetDepartmentId();

        var employee = await dbContext.Employees
            .Include(e => e.Department)
            .Include(e => e.Company)
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId && e.DepartmentId == departmentId, cts);

        if (employee is null)
            return NotFound<string>("There is no employee with this id.");

        var manager = await dbContext.Employees
            .Include(e => e.Department)
            .Include(e => e.Company)
            .FirstOrDefaultAsync(
            e => e.Id == request.ManagerId && e.CompanyId == companyId, cts);

        if (manager is null)
            return NotFound<string>("There is no manager with this id.");

        var canSetManager = employee.CanSetManager(manager, callerId, callerRole);

        if (!canSetManager)
            return Forbidden<string>("You are not authorize to do this action.");

        var IsValidManager = employee.IsValidManager(manager);

        if (!IsValidManager)
            return BadRequest<string>("This manager can not manage this employee.");

        employee.SetManager(manager.Id);

        await dbContext.SaveChangesAsync(cts);

        return Success("Manager is set successfully.");

    }

    public async Task<Response<string>> UnsetManagerAsync(string employeeId, string callerId, string callerRole, CancellationToken cts = default)
    {
        var companyId = tenantResolver.GetCompanyId();
        var departmentId = tenantResolver.GetDepartmentId();

        var employee = await dbContext.Employees
            .Include(e => e.Department)
            .Include(e => e.Company)
            .FirstOrDefaultAsync(e => e.Id == employeeId && e.DepartmentId == departmentId, cts);

        if (employee is null)
            return NotFound<string>("There is no employee with this id.");

        if (employee.Role == UserRole.CompanyManager)
            return BadRequest<string>("Company manager can not have manager to unset him.");

        var canUnsetManager = employee.CanUnsetManager(callerId, callerRole);

        if (!canUnsetManager)
            return Forbidden <string>("You are not authorized to do this action");


        employee.UnSetManager();

        await dbContext.SaveChangesAsync(cts);

        return Success("Manager is unset successfully.");
    }

}
