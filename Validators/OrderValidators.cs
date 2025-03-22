using FluentValidation;
using OrderManagementAPI.DTOs;

namespace OrderManagementAPI.Validators
{
    public class OrderValidators
    {
        public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
        {
            public CreateOrderDtoValidator()
            {
                RuleFor(x => x.CustomerName)
                    .NotEmpty().WithMessage("Customer name is required")
                    .MaximumLength(255).WithMessage("Customer name cannot exceed 255 characters");

                RuleFor(x => x.OrderDetails)
                    .NotEmpty().WithMessage("At least one order detail is required");

                RuleFor(x => x.Status)
                    .IsInEnum().WithMessage("Invalid order status.");

                // Add validation for each OrderDetail inside OrderDetails
                RuleForEach(x => x.OrderDetails)
                    .SetValidator(new CreateOrderDetailDtoValidator());
            }
        }

        public class UpdateOrderDtoValidator : AbstractValidator<UpdateOrderDto>
        {
            public UpdateOrderDtoValidator()
            {
                RuleFor(x => x.CustomerName)
                    .NotEmpty().WithMessage("Customer name is required")
                    .MaximumLength(255).WithMessage("Customer name cannot exceed 255 characters");

                RuleFor(x => x.Status)
                    .IsInEnum().WithMessage("Invalid order status.");
            }
        }

        public class CreateOrderDetailDtoValidator : AbstractValidator<CreateOrderDetailDto>
        {
            public CreateOrderDetailDtoValidator()
            {
                RuleFor(x => x.ProductName)
                    .NotEmpty().WithMessage("Product name is required")
                    .MaximumLength(255).WithMessage("Product name cannot exceed 255 characters");

                RuleFor(x => x.Quantity)
                    .GreaterThan(0).WithMessage("Quantity must be greater than 0");

                RuleFor(x => x.Price)
                    .GreaterThan(0).WithMessage("Price must be greater than 0")
                    .PrecisionScale(18, 2, false).WithMessage("TotalAmount can have up to 18 digits and 2 decimal places.");
            }
        }
    }
}