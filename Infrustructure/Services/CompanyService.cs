namespace Infrustructure.Services;

public sealed class CompanyService(
    AppDbContext dbContext,
    ITenantResolver tenantResolver,
    IDepartmentService departmentService,
    IDocumentChannel documentChannel) : ResponseHandler, ICompanyService
{
    public async Task<Response<string>> CreateAsync(CreateCompanyRequest request, CancellationToken cancellationToken = default)
    {
        var ownerId = tenantResolver.GetOwnerId();

        var existingCompany = await dbContext.Companies.FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);

        if (existingCompany is not null)
            return BadRequest<string>("Email is already registered.");

        var company = Company.Create(request.Name, request.Email, request.PhoneNumber, 
            request.Address, ownerId, request.ManagerId);

        await dbContext.Companies.AddAsync(company, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Created("Company created successfully.");
    }

    public async Task<Response<string>> DeleteAsync(string companyId, CancellationToken cts = default)
    {
        var ownerId = tenantResolver.GetOwnerId();

        var company = await dbContext.Companies
            .IgnoreQueryFilters()
            .Include(c => c.Departments)
            .FirstOrDefaultAsync(
            x => x.Id == companyId && x.OwnerId == ownerId , cts);

        if (company is null)
            return NotFound<string>("Company not found.");



        var departmentsIds = company.Departments.Select(d => d.Id).ToList();

        var documentsFilesPaths = await dbContext.Documents
                                                 .IgnoreQueryFilters()
                                                 .Where(d => d.CompanyId == companyId)
                                                 .Select(d => d.Path)
                                                 .ToListAsync(cts);

        var employeesIds = await dbContext.Employees.Where(d => d.CompanyId == companyId)
                                                 .Select(e => e.Id)
                                                 .ToListAsync(cts);

        tenantResolver.SetCompanyId(company.Id);

        foreach(var departmentId in departmentsIds)
            await departmentService.DeleteAsync(departmentId, cts);

        dbContext.Companies.Remove(company);

        await dbContext.SaveChangesAsync(cts);

        var @event = new CompanyIsDeletedEvent(company.Id, departmentsIds, employeesIds, documentsFilesPaths);

        await documentChannel.WriteCompanyIsDeletedAsync(@event, cts);

        return Deleted<string>("Company deleted successfully.");
    }

    public async Task<Response<List<CompanyResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var companies = await dbContext.Companies
                                       .Include(c => c.Manager)
                                       .Select(c => c.ToResponse())
                                       .ToListAsync(cancellationToken);

        if(companies is null || companies.Count == 0)
            return NotFound<List<CompanyResponse>>("No companies found for the owner.");

        return Success(companies);
    }

    public async Task<Response<CompanyResponse>> GetByIdAsync(string companyId, CancellationToken cancellationToken = default)
    {
        var company = await dbContext.Companies
                                     .FirstOrDefaultAsync(
                                     x => x.Id == companyId, cancellationToken);

        if (company is null)
            return NotFound<CompanyResponse>("Company not found.");

        return Success(company.ToResponse());
    }

    public async Task<Response<string>> UpdateAsync(UpdateCompanyRequest request, string companyId,  CancellationToken cancellationToken = default)
    {
        var company = await dbContext.Companies.FirstOrDefaultAsync(
            x => x.Id == companyId, cancellationToken);

        if (company is null)
            return NotFound<string>("Company not found.");

        company.Update(request.Name, request.Email, request.PhoneNumber, request.Address, request.ManagerId);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Success("Company updated successfully.");
    }

    public async Task<Response<EmployeeResponse>> GetManagerAsync(CancellationToken cts = default)
    {
        var companyId = tenantResolver.GetCompanyId();

        var manager = await dbContext.Employees
                                     .AsNoTracking()
                                     .IgnoreQueryFilters()
                                     .Include(e => e.Department)
                                     .FirstOrDefaultAsync(
            e => e.CompanyId == companyId && e.Role == UserRole.CompanyManager, cts);

        if (manager is null)
            return BadRequest<EmployeeResponse>("There is no employee with manager role.");

        return Success(manager.ToResponse());

    }

    public async Task<Response<string>> SetManagerAsync(string id, string managerId, CancellationToken cts = default)
    {
        var company = await dbContext.Companies.FirstOrDefaultAsync(c => c.Id == id, cts);

        if (company is null)
            return NotFound<string>("there is no company with this id.");

        if (company.ManagerId is not null)
            return BadRequest<string>("The company has already had a manager.");

        var manager = await dbContext.Employees.FirstOrDefaultAsync(
            e => e.Id == managerId && e.CompanyId == id, cts);

        if (manager is null)
            return NotFound<string>("there is no employee with this id.");

        if (manager.Role != UserRole.CompanyManager)
            return BadRequest<string>("The employee role must be company manager.");

        company.SetManager(manager.Id);

        await dbContext.SaveChangesAsync(cts);

        return Success("Company manager is set successfully.");

    }
    public async Task<Response<string>> UnSetManagerAsync(string id, CancellationToken cts = default)
    {

        var company = await dbContext.Companies.FirstOrDefaultAsync(c => c.Id == id, cts);

        if (company is null)
            return NotFound<string>("there is no company with this id.");

        if (company.ManagerId is null)
            return BadRequest<string>("The company has not already had a manager.");

        company.UnSetManager();

        await dbContext.SaveChangesAsync(cts);

        return Success("Company manager is unset successfully.");
    }
}
