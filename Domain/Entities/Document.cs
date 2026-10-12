
namespace Domain.Entities;

public class Document
{
    public string Id { get; set; }
    public string Name { get; private set; }
    public string Path { get; private set; }
    public string DepartmentId { get; init; }
    public Department Department { get; init; }
    public string? UploaderId { get; private set; }
    public Employee? Uploader { get; private set; }
    public string CompanyId { get; init; }
    public Company Company { get; init; }
    public string? ModifierId { get; private set; }
    public Employee? Modifier { get; private set; }

    public DateTime UploadedAt { get; init; }
    public DateTime? ModifiedAt { get; private set; }

    private Document() { }
    private Document(string name, string path, string departmentId, string uploaderId,string companyId) 
    {
        Id = Guid.NewGuid().ToString();
        Name = name;
        Path = path;
        DepartmentId = departmentId;
        CompanyId = companyId;
        UploaderId = uploaderId;
        UploadedAt = DateTime.UtcNow;
    }

    public static Document Create(string name, string path, string departmentId, string uploaderId, string companyId)
    {
        return new(name, path, departmentId, uploaderId, companyId);
    }

    public void UnsetUploader()
    {
        UploaderId = null;
    }
}
