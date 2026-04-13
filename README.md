**Digital Health and Fitness Tracker**  
**SE-410 Project**  
**Functional Requirements**

1. **User Registration & Authentication –** The system shall allow clients and trainers to register and log in with role-based access (Client or Trainer).  
2. **Exercise Logging –** Clients shall be able to log daily exercises including type, duration, sets, reps, and calories burned via the web interface.  
3. **Workout Program Management –** Trainers shall be able to create, update, and assign personalized workout programs to their clients **through the desktop application.**  
4. **Progress Tracking & Analytics –** The system shall use LINQ queries to analyze client progress over time (e.g., weekly averages, personal records, goal completion percentages).  
5. **Goal Setting & Tracking –** Clients shall be able to set fitness goals (weight loss, strength targets, weekly exercise frequency) and view their completion status.  
6. **Virtual Session Scheduling –** Clients shall be able to request and schedule virtual training sessions with their assigned trainer, selecting from available time slots.  
7. **Session Notification System –** The system shall use delegates and events to notify both trainers and clients about upcoming sessions, cancellations, and schedule changes.  
8. **Client Monitoring Dashboard –** Trainers shall be able to view a dashboard on the desktop app showing all assigned clients' recent activity, adherence rates, and progress summaries.  
9. **Health Data Recording –** Clients shall be able to log health metrics such as weight, heart rate, sleep hours, and water intake on a daily basis.  
10. **Progress Report Generation –** The system shall generate filtered and sorted progress reports using LINQ, allowing trainers to query client data by date range, exercise type, or goal category.

**Non-Functional Requirements**

1. **Performance –** LINQ-based progress queries and report generation shall return results within 3 seconds.  
2. **Usability –** The web interface for clients shall be intuitive enough for non-technical users to log exercises and view analytics without requiring training or documentation.  
3. **Scalability –** The database and application architecture shall support at least 50 concurrent clients and 10 trainers without degradation in response time.  
4. **Reliability –** Delegate-based notification events shall guarantee delivery to all subscribed handlers, and no session notification shall be silently dropped due to unhandled exceptions.  
5. **Security & Privacy –** All client health data shall be stored securely with role-based authorization ensuring trainers can only access data of their assigned clients, and clients cannot access other clients' data.  
   

