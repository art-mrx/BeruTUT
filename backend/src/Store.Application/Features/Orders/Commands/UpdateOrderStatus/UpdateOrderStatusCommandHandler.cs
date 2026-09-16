using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Store.Application.Common.Exceptions;
using Store.Application.Common.Interfaces;
using Store.Application.Features.Orders.Dtos;
using Store.Domain.Entities;

namespace Store.Application.Features.Orders.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, AdminOrderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateOrderStatusCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<AdminOrderDto> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.Id);

        order.Status = request.Status;
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<AdminOrderDto>(order);
    }
}
