using System.Collections.Generic;
using System.Linq;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Contracts.Commands.CreateContract;

public class CreateContractResponse
{
    private ContractDto? _singleContract;

    public List<ContractDto> Contracts { get; set; } = new();

    public ContractDto Contract
    {
        get => _singleContract ?? Contracts.FirstOrDefault()!;
        set
        {
            _singleContract = value;
            if (value != null && !Contracts.Contains(value))
            {
                Contracts.Insert(0, value);
            }
        }
    }
}