using MediatR;

namespace VendorManagementprojApplication.Features.Complaints.Commands.CreateComplaint;

public class CreateComplaintCommand : IRequest<CreateComplaintResponse>
{
    public int VendorID { get; set; }

    public int OutletID { get; set; }

    public int PurchaseOrderID { get; set; }

    public int POItemID { get; set; }

    public int ProductID { get; set; }

    public int RaisedByUserID { get; set; }

    public string ComplaintType { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Severity { get; set; } = "Medium";
}