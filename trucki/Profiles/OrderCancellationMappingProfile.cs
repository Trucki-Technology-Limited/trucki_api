using AutoMapper;
using trucki.Entities;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

public class OrderCancellationMappingProfile : Profile
{
    public OrderCancellationMappingProfile()
    {
        CreateMap<OrderCancellation, OrderCancellationResponseModel>()
            .ForMember(dest => dest.CancellationDetails, opt => opt.MapFrom(src => new CancellationDetails
            {
                CancelledAt = src.CancelledAt,
                CancellationReason = src.CancellationReason,
                PenaltyPercentage = src.PenaltyPercentage,
                PenaltyAmount = src.PenaltyAmount,
                RequiresApproval = false,
                PenaltyJustification = GetPenaltyJustification(src.PenaltyPercentage)
            }))
            .ForMember(dest => dest.RefundDetails, opt => opt.MapFrom(src => new RefundDetails
            {
                OriginalAmount = src.OriginalAmount,
                PenaltyAmount = src.PenaltyAmount,
                RefundAmount = src.RefundAmount,
                OriginalPaymentMethod = src.OriginalPaymentMethod ?? PaymentMethodType.Invoice,
                RefundMethod = src.RefundMethod, // Now uses the Entities version
                RefundStatus = src.Status == CancellationStatus.RefundProcessed ? "Processed" : "Pending",
                RefundProcessedAt = src.RefundProcessedAt
            }));

        CreateMap<CancelOrderRequestDto, OrderCancellation>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
    }

    private string GetPenaltyJustification(decimal penaltyPercentage)
    {
        return penaltyPercentage switch
        {
            0m => "No penalty as no driver has been selected",
            1m => "1% penalty applies as a driver has been selected", 
            2m => "2% penalty applies as a driver has been selected and acknowledged the order",
            _ => $"{penaltyPercentage}% penalty applies"
        };
    }
}