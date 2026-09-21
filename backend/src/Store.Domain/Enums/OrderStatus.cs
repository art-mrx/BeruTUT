namespace Store.Domain.Enums;

// Numeric values are persisted — only ever append new members, never renumber.
// New = awaiting the admin's phone confirmation; Cancelled = rejected by the admin.
public enum OrderStatus
{
    New = 0,
    Processing = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4,
    Approved = 5
}
