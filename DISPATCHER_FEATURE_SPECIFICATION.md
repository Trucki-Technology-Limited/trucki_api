# Dispatcher Feature Specification

## Document Overview
**Feature Name**: US Market Dispatcher System
**Version**: 1.0
**Date**: October 2025
**Document Type**: Non-Technical Feature Specification

---

## 1. Executive Summary

### 1.1 Feature Overview
The Dispatcher System introduces a new user type for the US market that enables transportation companies to manage multiple drivers and bid on cargo orders on their behalf. This feature bridges the gap between independent drivers and large fleet operations, providing a commission-based model similar to the existing Nigerian transporter system but tailored for US market regulations and workflows.

### 1.2 Business Problem
Currently, the US market only supports independent drivers who bid directly on cargo orders. Many transportation companies operate with multiple drivers but lack a way to:
- Centrally manage their driver fleet
- Bid on orders on behalf of their drivers
- Earn commission from successful deliveries
- Maintain compliance with US transportation regulations

### 1.3 Business Value
- **New Revenue Stream**: Commission-based earnings for dispatcher companies
- **Market Expansion**: Attract larger transportation companies to the platform
- **Improved Efficiency**: Centralized bidding and driver management
- **Regulatory Compliance**: Proper documentation and approval workflows for US market

---

## 2. User Personas & Stakeholders

### 2.1 Primary Users

#### Dispatcher
- **Role**: Transportation company owner/manager
- **Location**: United States
- **Needs**: Manage multiple drivers, bid on orders, earn commission
- **Goals**: Maximize fleet utilization and profitability

#### Cargo Owner
- **Role**: Business shipping goods
- **Experience**: Should see no difference from current driver bidding
- **Expectation**: Transparent interaction with actual drivers

#### Driver (Dispatcher-Managed)
- **Role**: Professional truck driver
- **Relationship**: Employed or contracted by dispatcher
- **Income**: Receives percentage of order value after dispatcher commission

### 2.2 Secondary Users

#### System Administrator
- **Role**: Platform oversight
- **Responsibilities**: Approve new drivers, monitor compliance
- **Access**: Full visibility into dispatcher operations

---

## 3. Feature Requirements

### 3.1 Core Functionality

#### 3.1.1 Dispatcher Registration & Management
**Description**: Dispatchers can register on the platform and manage their business profile.

**User Story**: *"As a transportation company owner, I want to register as a dispatcher so that I can manage multiple drivers and bid on orders on their behalf."*

**Key Features**:
- Registration with company details and US business documentation
- Country selection (automatic for US dispatchers)
- Bank account setup for commission payments
- Business profile management

**Acceptance Criteria**:
- Dispatchers must provide valid US business documentation
- Profile includes company name, address, contact information
- Integration with existing user management system

#### 3.1.2 Driver Onboarding & Management
**Description**: Dispatchers can add, onboard, and manage drivers with complete documentation.

**User Story**: *"As a dispatcher, I want to add and fully onboard drivers including all required documents and truck details so they can start accepting orders."*

**Key Features**:
- Multi-step driver onboarding process
- Automatic country inheritance from dispatcher
- Document management for US compliance requirements
- Truck registration and linking
- Commission structure setup per driver

**Acceptance Criteria**:
- All US-required documents must be uploaded and verified
- Each driver must have a truck assigned during onboarding
- Commission percentage can be set individually per driver (0-50%)
- Admin approval required before drivers become active

#### 3.1.3 Order Bidding System
**Description**: Dispatchers can view available orders and submit bids on behalf of their drivers.

**User Story**: *"As a dispatcher, I want to select one of my drivers and bid on cargo orders so that I can maximize my fleet utilization and earn commission."*

**Key Features**:
- View cargo orders available in dispatcher's region
- Select specific driver for each bid
- Submit competitive bid amounts
- Track bid status and responses
- Cargo owners see simple indicator that bid came from dispatcher

**Acceptance Criteria**:
- Only approved drivers appear in selection list
- Bid submission creates transparent driver-focused bid for cargo owners
- Commission automatically calculated and tracked
- Cargo owners see dispatcher indicator but not dispatcher identity
- Clear distinction between direct driver bids and dispatcher-submitted bids

### 3.2 Advanced Features

#### 3.2.1 Commission Management
**Description**: Flexible commission structure with automatic calculation and payment processing.

**User Story**: *"As a dispatcher, I want to set different commission rates for each driver and automatically receive my earnings when orders are completed."*

**Key Features**:
- Individual commission rates per driver
- Automatic commission calculation on order completion
- Payment splitting between driver and dispatcher
- Commission history and reporting

**Acceptance Criteria**:
- Commission rates can be updated with historical tracking
- Payments processed automatically upon order completion
- Clear earning breakdowns for both dispatcher and driver

#### 3.2.2 Order Document Management
**Description**: Dispatchers can help drivers upload cargo order documents during delivery process.

**User Story**: *"As a dispatcher, I want to upload manifest and delivery documents on behalf of my drivers so that I can ensure timely order completion and maintain control over the delivery process."*

**Key Features**:
- Upload manifest documents to start order transit
- Upload delivery documents to complete orders
- Same workflow and status changes as driver uploads
- Automatic commission processing upon order completion

**Acceptance Criteria**:
- Only dispatchers managing the assigned driver can upload documents
- Manifest upload triggers order status change to "In Transit"
- Delivery document upload completes order and triggers payment processing
- Commission automatically split between driver and dispatcher

#### 3.2.3 Fleet Management Dashboard
**Description**: Comprehensive dashboard for managing multiple drivers and their activities.

**User Story**: *"As a dispatcher, I want a dashboard to view all my drivers' statuses, active orders, and performance metrics."*

**Key Features**:
- Driver status overview (available, busy, pending approval)
- Active order tracking with document upload capabilities
- Performance metrics and earnings summaries
- Driver document expiration alerts

---

## 4. User Experience & Workflows

### 4.1 Dispatcher Registration Flow
1. **Sign Up**: Dispatcher visits registration page and selects "Dispatcher" role
2. **Company Information**: Enters business details, selects United States as country
3. **Documentation**: Uploads required business documents
4. **Bank Setup**: Provides banking information for commission payments
5. **Verification**: Admin reviews and approves dispatcher account
6. **Activation**: Dispatcher receives confirmation and can begin adding drivers

### 4.2 Driver Onboarding Flow
1. **Initial Setup**: Dispatcher enters basic driver information (name, email, contact)
2. **Commission Setup**: Sets commission percentage for the driver
3. **Document Upload**:
   - Driver receives login credentials
   - Dispatcher helps upload all required US documents (DOT number, license, etc.)
   - System validates against country-specific requirements
4. **Truck Registration**:
   - Add truck details and documentation
   - Upload truck photos and measurements
   - Link truck directly to the driver
5. **Final Review**: System validates all requirements are met
6. **Admin Approval**: Submitted to admin for final approval
7. **Activation**: Driver becomes available for bidding once approved

### 4.3 Order Bidding Flow
1. **Browse Orders**: Dispatcher views available cargo orders in their region
2. **Driver Selection**: For each order, dispatcher selects appropriate driver
3. **Bid Submission**: Enters competitive bid amount and any notes
4. **Cargo Owner Review**: Cargo owner sees bid from driver (dispatcher invisible)
5. **Selection**: If selected, driver is notified and begins order execution
6. **Commission Processing**: Upon completion, earnings automatically split

### 4.4 Cargo Owner Experience
1. **View Bids**: Sees bids from drivers with simple dispatcher indicator
2. **Driver Evaluation**: Reviews driver profiles, truck details, and ratings
3. **Bid Assessment**: Can identify which bids came from dispatchers vs. direct drivers
4. **Selection**: Chooses preferred driver based on standard criteria
5. **Order Execution**: Interacts directly with driver throughout delivery
6. **Payment**: Pays order amount (commission split happens automatically)

### 4.5 Order Document Upload Flow
1. **Order Assignment**: Driver is selected for cargo order through dispatcher bid
2. **Manifest Upload Options**:
   - Driver can upload manifest documents directly
   - Dispatcher can upload manifest documents on behalf of driver
   - Either action triggers order status change to "In Transit"
3. **Delivery Completion Options**:
   - Driver can upload delivery documents directly
   - Dispatcher can upload delivery documents on behalf of driver
   - Either action completes order and triggers commission processing
4. **Payment Processing**: System automatically splits earnings between driver and dispatcher

---

## 5. Business Rules & Constraints

### 5.1 Operational Rules
- Dispatchers can only operate in their registered country
- Drivers automatically inherit dispatcher's country
- Only one truck per driver during onboarding (unlike transporter model)
- Commission rates are capped at 50% maximum
- All drivers must pass admin approval before becoming active

### 5.2 Regulatory Compliance
- All US transportation documentation requirements must be met
- DOT numbers mandatory for US drivers
- Document expiration tracking and renewal alerts
- Regular compliance audits for dispatcher-managed drivers

### 5.3 Platform Integration
- Seamless integration with existing cargo order system
- No disruption to current driver or cargo owner experiences
- Backward compatibility with all existing features
- Consistent user interface patterns across user types

---

## 6. Success Metrics

### 6.1 Adoption Metrics
- Number of registered dispatchers within first 6 months
- Average number of drivers per dispatcher
- Percentage of dispatchers who complete driver onboarding

### 6.2 Business Metrics
- Total commission volume generated
- Order completion rates for dispatcher-managed drivers
- Customer satisfaction scores for dispatcher-originated orders

### 6.3 Platform Health
- No degradation in existing user experience metrics
- System performance maintained under increased load
- Zero security incidents related to new features

---

## 7. Implementation Timeline

### 7.1 Phase 1: Foundation (Weeks 1-2)
- Database updates and core entity modifications
- Basic dispatcher registration functionality
- Initial driver creation workflow

### 7.2 Phase 2: Core Features (Weeks 3-4)
- Complete driver onboarding process
- Document management integration
- Basic commission structure

### 7.3 Phase 3: Bidding & Operations (Weeks 5-6)
- Order bidding functionality
- Role-based bid visibility
- Commission calculation and processing

### 7.4 Phase 4: Polish & Launch (Weeks 7-8)
- User interface refinements
- Comprehensive testing
- Documentation and training materials
- Soft launch with select users

---

## 8. Risk Assessment & Mitigation

### 8.1 Technical Risks
**Risk**: Complexity of role-based bid visibility
**Mitigation**: Thorough testing with different user types, clear separation of concerns

**Risk**: Commission calculation accuracy
**Mitigation**: Comprehensive unit testing, audit trails, manual verification capabilities

### 8.2 Business Risks
**Risk**: Cargo owner confusion about dispatcher vs. driver
**Mitigation**: Maintain driver-focused interface, clear communication about who they're working with

**Risk**: Regulatory compliance challenges
**Mitigation**: Work with legal team on US transportation requirements, flexible document system

### 8.3 User Experience Risks
**Risk**: Complex onboarding process deterring dispatchers
**Mitigation**: Step-by-step guidance, progress indicators, dedicated support during beta

---

## 9. Support & Training Requirements

### 9.1 User Documentation
- Dispatcher onboarding guide
- Driver management best practices
- Commission structure explanation
- Troubleshooting common issues

### 9.2 Customer Support
- Dedicated support channel for dispatchers during launch
- Training for support team on new workflows
- Escalation procedures for compliance-related issues

### 9.3 Training Materials
- Video tutorials for driver onboarding process
- Webinar series for new dispatchers
- FAQ documentation for all user types

---

## 10. Future Enhancements

### 10.1 Planned Improvements
- Automated driver assignment based on proximity and availability
- Advanced fleet analytics and reporting
- Integration with external fleet management systems
- Multi-country expansion for dispatcher model

### 10.2 Potential Features
- Driver performance scoring and recommendations
- Bulk order bidding capabilities
- White-label solutions for large transportation companies
- API access for enterprise dispatchers

---

## 11. Conclusion

The Dispatcher System represents a significant expansion of the platform's capabilities, enabling larger transportation companies to participate in the US market while maintaining the transparent, driver-focused experience that cargo owners expect. The phased implementation approach ensures minimal disruption to existing users while providing powerful new tools for fleet management and commission-based earnings.

This feature positions the platform as a comprehensive solution for all sizes of transportation operations, from individual drivers to large fleet operators, ultimately driving increased market share and revenue growth in the competitive US logistics market.