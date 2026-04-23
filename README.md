<p align="center">
  <img src="images/logo_p1_1.png" alt="Digital Health and Fitness Tracker Logo" width="120"/>
</p>

# Digital Health and Fitness Tracker
## SE-410 Project

## Functional Requirements

1. **User Registration & Authentication** – The system shall allow clients and trainers to register and log in with role-based access (Client or Trainer).
2. **Exercise Logging** – Clients shall be able to log daily exercises including type, duration, sets, reps, and calories burned via the web interface.
3. **Workout Program Management** – Trainers shall be able to create, update, and assign personalized workout programs to their clients **through the desktop application.**
4. **Progress Tracking & Analytics** – The system shall use LINQ queries to analyze client progress over time (e.g., weekly averages, personal records, goal completion percentages).
5. **Goal Setting & Tracking** – Clients shall be able to set fitness goals (weight loss, strength targets, weekly exercise frequency) and view their completion status.
6. **Virtual Session Scheduling** – Clients shall be able to request and schedule virtual training sessions with their assigned trainer, selecting from available time slots.
7. **Session Notification System** – The system shall use delegates and events to notify both trainers and clients about upcoming sessions, cancellations, and schedule changes.
8. **Client Monitoring Dashboard** – Trainers shall be able to view a dashboard on the desktop app showing all assigned clients' recent activity, adherence rates, and progress summaries.
9. **Health Data Recording** – Clients shall be able to log health metrics such as weight, heart rate, sleep hours, and water intake on a daily basis.
10. **Progress Report Generation** – The system shall generate filtered and sorted progress reports using LINQ, allowing trainers to query client data by date range, exercise type, or goal category.

## Non-Functional Requirements

1. **Performance** – LINQ-based progress queries and report generation shall return results within 3 seconds.
2. **Usability** – The web interface for clients shall follow a consistent color-coding scheme to improve intuitive understanding: green for achieved goals and completed exercises, yellow for in-progress or 
partially completed activities, red for missed goals.
3. **Scalability** – The database and application architecture shall support at least 50 concurrent clients and 10 trainers without degradation in response time.
4. **Reliability** – Delegate-based notification events shall guarantee delivery to all subscribed handlers, and no session notification shall be silently dropped due to unhandled exceptions.
5. **Security & Privacy** – All client health data shall be stored securely with  role-based authorization ensuring trainers can only access data of their assigned clients, and clients cannot access other clients' data. User passwords shall be hashed using firebase's hash algorithm.

## Scenario

  The client first registers in the Digital Health and Fitness Tracker system 
and logs in securely with role-based access. During daily use, the client records 
exercises such as workout type, duration, sets, reps, and calories burned, and 
also enters health data like weight, heart rate, sleep hours, and water intake. 
Based on this information, the client sets fitness goals like weekly exercise 
targets  or  weight  loss  goals,  and  the  system  tracks  progress  using  simple 
indicators for completed, ongoing, or missed goals. 

  Each  trainer  logs  in  through  the  desktop  application  and  creates 
personalized workout programs for assigned clients. Trainers can update these 
programs anytime according to the client’s progress. They can also monitor all 
assigned clients from a dashboard that shows recent activities, adherence rates, 
and progress summaries, making it easier to follow each client’s development. 
Clients can request virtual training sessions by choosing available time 
slots  from  their  trainer’s  schedule.  When  a  session  is  created,  updated,  or 
canceled, the system uses event-based notifications to inform both the trainer 
and the client. This helps both sides stay updated without missing any schedule 
changes. 

  The  system  analyzes  exercise and health data using LINQ queries to 
calculate weekly averages, personal records, and goal completion percentages. 
Trainers can also generate progress reports by filtering client data by exercise 
type,  date  range,  or  goal  category.  All  reports  and  analytics  are  generated 
quickly,  while  secure  authorization  ensures  trainers  can  only  access  their 
assigned clients’ data, protecting privacy and maintaining system security. 
 
 