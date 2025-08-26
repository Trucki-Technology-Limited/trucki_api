using trucki.Entities;

namespace trucki.Models.ResponseModels
{
    public class CargoFinancialSummaryResponse
    {
        // === CORE REVENUE METRICS ===
        public decimal TotalSystemFees { get; set; }         // Our 15% commission revenue
        public decimal TotalTaxCollected { get; set; }       // Tax revenue if applicable  
        public decimal TotalCompanyRevenue { get; set; }     // SystemFees + Tax
        
        // === CUSTOMER PAYMENT TRACKING ===
        public decimal TotalCustomerPayments { get; set; }   // All payments received
        public decimal TotalDriverPayouts { get; set; }      // Total paid to drivers
        public decimal PendingDriverPayouts { get; set; }    // Unpaid driver earnings
        
        // === PAYMENT METHOD BREAKDOWN ===
        public PaymentMethodBreakdown PaymentMethods { get; set; }
        
        // === INVOICE STATUS SUMMARY ===
        public InvoiceStatusSummary InvoicesSummary { get; set; }
        
        // === CUSTOMER TYPE ANALYSIS ===
        public CustomerTypeFinancials ShipperStats { get; set; }
        public CustomerTypeFinancials BrokerStats { get; set; }
        
        // === PERFORMANCE METRICS ===
        public PerformanceMetrics Performance { get; set; }
        
        // === MONTHLY TRENDS ===
        public List<MonthlyFinancialData> MonthlyBreakdown { get; set; }
        
        // === METADATA ===
        public DateTime GeneratedAt { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Period { get; set; }
    }

    public class PaymentMethodBreakdown
    {
        public decimal StripePayments { get; set; }
        public decimal WalletPayments { get; set; }
        public decimal InvoicePayments { get; set; }
        public decimal MixedPayments { get; set; }
        
        // Percentages
        public decimal StripePercentage { get; set; }
        public decimal WalletPercentage { get; set; }
        public decimal InvoicePercentage { get; set; }
        public decimal MixedPercentage { get; set; }
    }

    public class InvoiceStatusSummary
    {
        public decimal PendingAmount { get; set; }          // Unpaid invoices
        public decimal OverdueAmount { get; set; }          // Past-due invoices
        public decimal PaidAmount { get; set; }             // Completed payments
        public decimal CancelledAmount { get; set; }        // Cancelled invoices
        
        public int PendingCount { get; set; }
        public int OverdueCount { get; set; }
        public int PaidCount { get; set; }
        public int CancelledCount { get; set; }
        public int TotalInvoices { get; set; }
        
        // Health metrics
        public decimal CollectionRate { get; set; }         // Paid / (Paid + Pending + Overdue)
        public decimal OverdueRate { get; set; }            // Overdue / Total
    }

    public class CustomerTypeFinancials
    {
        public string CustomerType { get; set; }            // "Shipper" or "Broker"
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public decimal TotalRevenue { get; set; }           // System fees from this type
        public decimal TotalPayments { get; set; }          // Total payments received
        public decimal PendingPayments { get; set; }        // Outstanding payments
        public decimal OverduePayments { get; set; }        // Past due payments
        public decimal AverageOrderValue { get; set; }      // Average bid amount
        public decimal AverageSystemFee { get; set; }       // Average commission
        public decimal PaymentSuccessRate { get; set; }     // % of orders paid
        public double AveragePaymentDays { get; set; }      // Days from order to payment
    }

    public class PerformanceMetrics
    {
        public decimal RevenueGrowthRate { get; set; }       // Month-over-month growth
        public decimal AverageRevenuePerOrder { get; set; }  // System fee / order
        public decimal PaymentSuccessRate { get; set; }      // Overall payment success
        public double AverageCollectionTime { get; set; }    // Days to collect payment
        public decimal CustomerRetentionRate { get; set; }   // Returning customers %
        public int ActiveCustomers { get; set; }             // Customers with orders in period
        public int NewCustomers { get; set; }                // First-time customers
    }

    public class MonthlyFinancialData
    {
        public string Month { get; set; }                    // "Jan 2025"
        public int Year { get; set; }
        public int MonthNumber { get; set; }
        public decimal Revenue { get; set; }                 // System fees earned
        public decimal CustomerPayments { get; set; }        // Total payments received
        public decimal DriverPayouts { get; set; }           // Paid to drivers
        public decimal NewInvoices { get; set; }             // Invoices created
        public decimal InvoicesPaid { get; set; }            // Invoices settled
        public int CompletedOrders { get; set; }
        public int NewCustomers { get; set; }
        public decimal ShipperRevenue { get; set; }          // Revenue from shippers
        public decimal BrokerRevenue { get; set; }           // Revenue from brokers
    }
}
