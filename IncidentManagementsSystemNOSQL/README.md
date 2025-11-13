# Student name: Darlington Jones
# Student Number: 711336
# Individual Task: Forget Password Feature
# Login Details for this task:
# Username: marya.butt
# Password: marya45

# 1. User Flow – How the Feature Works
# Step 1 – User selects “Forget Password”

From the Login page, the user clicks “Forget password”.

# Step 2 – User enters their username

They are taken to the Forgot Password page, where they enter their username
(example: marya.butt) and submit the form.

No matter what username is entered, the system always displays the same confirmation message:

“If the username exists, we have sent a reset link.”

This protects user privacy.

# Step 3 – System sends a reset email

The application generates a unique, secure token and sends an email using the SMTP configuration in the project.

The email contains a reset link in this format:

https://localhost:7039/Account/ResetPassword?uid=...&token=...

# Step 4 – User opens the link and resets the password

The link opens the Reset Password page where the user enters:

New Password

Confirm Password

Password requirements:

Minimum 6 characters

Both fields must match

# Step 5 – Reset completes

If successful, the system updates the user's password and redirects them back to the Login page with a success message.

# Step 6 – User logs in with the new password
This confirms the end-to-end reset process works.

# 2. Components and Classes I Implemented

Below are the exact classes involved in building the Reset Password feature:

Controller

AccountController.cs
Handles ForgotPassword and ResetPassword actions, token validation, and form submission.

Models / ViewModels

ForgotPasswordViewModel.cs – Captures the username.

ResetPasswordViewModel.cs – Holds UserId, Token, NewPassword, ConfirmPassword.

SetPasswordViewModel.cs – Used for first-time password setup.

Models

PasswordResetToken.cs – Stores reset tokens in MongoDB with expiration time.

Repositories

IPasswordResetTokenRepository.cs

PasswordResetTokenRepository.cs
Store and retrieve password reset tokens.

Services

IPasswordResetService.cs

PasswordResetService.cs
Generate tokens, validate tokens, and interact with the repository.

IEmailSender.cs

SmtpEmailSender.cs
Sends the actual reset email through SMTP.

IPasswordHasher.cs

PasswordHasher.cs
Securely hashes the new password.

These components form the complete backend logic for resetting a password.

# 3. Views I Created or Updated

All UI pages for the Reset Password feature are inside:

Views → Account

ForgotPassword.cshtml

ForgotPasswordConfirmation.cshtml

ResetPassword.cshtml

Login.cshtml (updated navigation and styling)

# ...............................................................................................................................................

Student Name: Noku Udinge
Student Number: 727573

Ticket Sorting by Priority Level
Feature Overview
The ticket management system includes a priority-based filtering feature that allows users to view and sort tickets based on their priority levels: High, Medium, and Low.
Functionality

Filter by Priority: Users can filter the ticket list to display only tickets of a specific priority level
Priority Statistics: The dashboard displays real-time counts for each priority level
Visual Indicators: Each priority level is color-coded with distinct badges for easy identification:

 -High Priority - Dark green gradient for urgent tickets
🟡 Medium Priority - Medium green gradient for standard tickets
- Low Priority - Light green gradient for non-urgent tickets
🟦 All Tickets - View all tickets regardless of priority



How to Use

Navigate to the Tickets page (/Ticket/Index)
Click on one of the priority stat cards at the top of the page, or
Use the dropdown filter in the filter section to select a specific priority level
The ticket list will automatically update to show only tickets matching the selected priority
Click "All Tickets" or select "All Priorities" from the dropdown to view the complete list

Technical Implementation

Route: GET /Ticket/Index?priority={Low|Medium|High}
Priority Enum: TicketPriority enum with values: Low, Medium, High
Filter Logic: Server-side filtering in the Ticket Controller based on query parameter
Real-time Counts: Priority statistics are calculated and displayed at the top of the page
Persistence: Selected filter persists across page refreshes via URL parameter


# ...................................................................................................................................................................

# Student Name: Efe Kodzhagioz 
# Student Number: 733456

# Individual Contribution: Client‑Side Filtering of Preloaded Ticket Lists

## What it does
- Adds instant, in‑browser filtering for ticket tables that are already rendered by the server.
- Supports free‑text search and structured filters (status, department, “my tickets” toggle, quick keyword terms).
- Debounces input to keep the UI responsive.
- Updates visible ticket counts without reloading the page or calling APIs.

## Where it is used
- Dashboard view: IncidentManagementsSystemNOSQL/Views/Dashboard/Index.cshtml
  - Marker comment: Efe Individual Part Filtering a preloaded list
- Employee tickets view: IncidentManagementsSystemNOSQL/Views/Employee/Index.cshtml
  - Marker comment: Efe Individual Part Filtering a preloaded list

## High‑level design
- Server renders all tickets (or the current slice) into a table in Razor.
- Each row includes data attributes and visible text that the JavaScript filter logic can match against (e.g., status, department, ownership).
- Lightweight vanilla JavaScript reads input fields, applies case‑insensitive matching, and shows/hides rows accordingly.
- A small debounce (≈120 ms) smooths typing.
- A badge in the card header is updated to reflect the number of rows currently shown.

## Implementation details

### Dashboard (Views/Dashboard/Index.cshtml)
- Inputs and controls
  - Search box: #searchInput (matches across row text)
  - Status dropdown: #statusFilter
  - Department dropdown: #departmentFilter
  - “My tickets” only: #assignedSwitch (when agent context is present)
  - Quick keyword filter: #ticketsQuickFilter (space‑separated terms; all must match)
- Table and counters
  - Table: #ticketsTable
  - Visible count badge: #ticketCount
- Row metadata (added to each <tr>)
  - data-status → ticket status string (e.g., open, in_progress, closed_resolved, closed_no_resolve)
  - data-department → department display name (e.g., IT Support)
  - data-assigned-state → mine | other | unassigned (used by the “My tickets” switch)
- Core logic
  - Reads current filter values, lowers cases for case‑insensitive compare.
  - Calculates for each row:
    - matchesSearch: row text contains #searchInput value
    - matchesQuick: every term in #ticketsQuickFilter appears in row text
    - matchesStatus: empty or equals data-status
    - matchesDepartment: empty or equals data-department
    - matchesAssigned: if switch on, data-assigned-state === 'mine'
  - Sets row.style.display = 'none' for non‑matches; increments visible counter for matches.
  - Updates #ticketCount text to “N Tickets”.
  - Debounces the quick filter to 120 ms.

### Employee (“My Tickets”) (Views/Employee/Index.cshtml)
- Inputs and controls
  - Search box: #employeeTicketsSearch
  - Keyword filter: #employeeTicketsFilter (space‑separated terms)
- Table and counters
  - Table: #employeeTicketsTable
  - Visible count badge: first .tickets-card .badge.bg-primary within the card
- Core logic
  - Collects all <tr> rows once (preloaded list), converts text to lower case.
  - On input, debounced by 120 ms, compares:
    - matchesSearch: row text includes search value
    - matchesKeywords: every term from keyword filter appears in row text
  - Sets row.hidden = true for non‑matches; counts visible rows.
  - Updates the card badge text to “N shown”.

## How to use (for end users)
- Dashboard page:
  - Type in “Search tickets…” to filter by ticket id, title, employee name/email, etc.
  - Narrow by Status and Department.
  - Toggle “My tickets” to see only tickets assigned to you (when available).
  - Use the “Quick filter” box for multiple terms; all terms must match.
- Employee page:
  - Use the top search box to match id/title/assignee text.
  - Add keywords in the second box; each term further narrows results.
  - The badge shows how many rows are currently visible.

## Developer notes and extension points
- Adding a new filter on Dashboard:
  1) Add a control (input/select) with an id.
  2) Populate any needed data-* attributes on table rows to support that filter.
  3) Update the filterTable() function to compute a matchesX condition and include it in the final conjunction.
- Changing status/department values:
  - Ensure the options in the selects match the exact strings set in data-status and data-department for rows. Keep casing consistent or normalize to lower case for comparisons.
- Debounce tuning:
  - Both pages use ~120 ms. Increase for very large tables to reduce work while typing, or decrease for snappier feedback.
- Performance tips:
  - Keep row text lean; prefer data-* attributes for values you filter on to avoid expensive text scans.
  - If tables get very large (thousands of rows), consider virtualized rendering or server‑side filtering endpoints.
- Null/empty values:
  - Code defensively uses empty strings when attributes are missing.
  - When extending, coalesce potential nulls before comparing.

## Testing checklist (manual)
- Typing in each input filters rows immediately (after small delay) without page reload.
- Clearing an input restores hidden rows.
- Status/department filters match the row’s data exactly after normalization.
- “My tickets” switch hides everything except rows with data-assigned-state="mine".
- The badge count updates as expected and matches the number of visible rows.

## Known limitations
- Filtering is purely client‑side; it only affects what’s already rendered.
- No pagination/virtualization; very large tables may degrade performance.
- Matching is simple substring search (case‑insensitive). For advanced search (phrases, negation), consider enhancing the logic or moving to server‑side search.

## Code references (quick map)
- Dashboard script: Views/Dashboard/Index.cshtml — filterTable() and input event wiring under @section Scripts { ... }.
- Employee script: Views/Employee/Index.cshtml — self‑invoking function wiring inputs and applyFilter().
- Row attributes (Dashboard): data-status, data-department, data-assigned-state added in the <tbody> foreach.
- Badge selectors: #ticketCount (Dashboard), .tickets-card .badge.bg-primary (Employee).