# RaceDay API Endpoint Plan

This endpoint plan is designed before Part 2 implementation. Routes use the `/api/` prefix and role-based access is enforced at the API level.

| HTTP Method | Route | Description | Role Required | Request Body (if any) | Expected Response |
|---|---|---|---|---|---|
| POST | `/api/auth/register` | Creates a new RaceDay user account. | None (public) | `{ fullName, email, password, role, phoneNumber }` | **201 Created** – user created; **400 Bad Request** – validation/duplicate email |
| POST | `/api/auth/login` | Authenticates a user and returns an access token. | None (public) | `{ email, password }` | **200 OK** – token and user details; **401 Unauthorized** – invalid credentials |
| GET | `/api/users/me` | Returns the authenticated user's profile. | Any logged-in user | None | **200 OK** – profile; **401 Unauthorized** |
| PUT | `/api/users/me` | Updates the authenticated user's profile. | Any logged-in user | `{ fullName, phoneNumber, password? }` | **200 OK** – updated profile; **400 Bad Request** |
| GET | `/api/events` | Lists upcoming RaceDay events. | None (public) | None | **200 OK** – event list |
| GET | `/api/events/{id}` | Returns details for one event. | None (public) | None | **200 OK** – event; **404 Not Found** |
| POST | `/api/events` | Creates a new event owned by the organiser. | Organiser | Event details | **201 Created**; **400 Bad Request** |
| PUT | `/api/events/{id}` | Edits an event owned by the organiser. | Organiser | Event fields to update | **200 OK**; **403 Forbidden**; **404 Not Found** |
| DELETE | `/api/events/{id}` | Deletes an event owned by the organiser. | Organiser | None | **204 No Content**; **403 Forbidden**; **404 Not Found** |
| GET | `/api/events/{eventId}/categories` | Lists categories available for an event. | None (public) | None | **200 OK**; **404 Not Found** |
| POST | `/api/events/{eventId}/categories` | Adds a category to an event. | Organiser | `{ categoryName, distanceKm, entryFee, maxParticipants }` | **201 Created**; **400 Bad Request** |
| PUT | `/api/categories/{id}` | Updates an event category. | Organiser | Category fields | **200 OK**; **403 Forbidden**; **404 Not Found** |
| DELETE | `/api/categories/{id}` | Deletes an event category if it is not in use. | Organiser | None | **204 No Content**; **409 Conflict** if in use |
| POST | `/api/events/{eventId}/enrolments` | Enrols the logged-in participant in a selected category. | Participant | `{ categoryId }` | **201 Created**; **404 Not Found**; **409 Conflict** if duplicate/full |
| GET | `/api/enrolments/me` | Lists the participant's own enrolments. | Participant | None | **200 OK** – enrolment list |
| DELETE | `/api/enrolments/{id}` | Cancels the participant's own enrolment. | Participant | None | **204 No Content**; **403 Forbidden**; **404 Not Found** |
| GET | `/api/events/{eventId}/enrolments` | Lists all enrolments for an organiser's event. | Organiser | None | **200 OK**; **403 Forbidden** |
| GET | `/api/results/me` | Returns the participant's personal results/history. | Participant | None | **200 OK** – results list |
| GET | `/api/events/{eventId}/results` | Returns results for an organiser's event. | Organiser | None | **200 OK**; **403 Forbidden** |
| POST | `/api/events/{eventId}/results` | Captures a participant result. | Organiser | `{ enrolmentId, finishTime, position, status }` | **201 Created**; **400/404** |
| PUT | `/api/results/{id}` | Corrects or updates a result. | Organiser | `{ finishTime, position, status }` | **200 OK**; **403/404** |
| DELETE | `/api/results/{id}` | Removes an incorrectly captured result. | Organiser | None | **204 No Content**; **403/404** |
| GET | `/api/events/{eventId}/route` | Returns route information for an event. | None (public) | None | **200 OK**; **404 Not Found** |
| POST | `/api/events/{eventId}/route` | Adds route information to an organiser's event. | Organiser | `{ routeName, distanceKm, routeUrl, description }` | **201 Created**; **403 Forbidden** |
| PUT | `/api/routes/{id}` | Updates route information. | Organiser | Route fields | **200 OK**; **403/404** |
| GET | `/api/events/{eventId}/weather` | Returns live weather information for the event using a weather service. | None (public) | None | **200 OK**; **404**; **502 Bad Gateway** if provider fails |

## Role rules

- **Public:** register/login, event browsing/details, category browsing, route details and weather.
- **Participant:** profile, own enrolments, enrolment creation/cancellation and own results.
- **Organiser:** create/edit/delete own events, manage categories/routes for own events, view event enrolments and capture/update/delete results.
- Users must not modify another organiser's event or access another participant's private enrolments/results.
