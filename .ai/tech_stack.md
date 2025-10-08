# Technology Stack for 10xWarehouse

This document outlines the technology stack chosen for the 10xWarehouse project.

## 1. Frontend

-   Framework: Astro
-   UI Components: Vue.js
-   Styling: Tailwind CSS
-   UI Library: shadcn/ui

Astro is used as the primary framework for its performance benefits and content-focused architecture. Vue.js will be integrated within Astro to handle dynamic, interactive UI components for the application's dashboard and management interfaces. Styling will be implemented using Tailwind CSS for a utility-first workflow, complemented by shadcn/ui for high-quality, pre-built components.

## 2. Backend

-   Framework: .NET

The backend will be a RESTful API built on the .NET platform. This choice provides a robust, scalable, and high-performance foundation for the application's business logic, data access, and API endpoints.

## 3. Database

-   Database: PostgreSQL

PostgreSQL is the chosen relational database. It is known for its reliability, feature robustness, and strong support for complex queries, making it a suitable choice for managing the structured data of a warehouse management system.

## 4. Services & Authentication

-   Authentication: Supabase

User authentication will be handled by Supabase. Leveraging a third-party service like Supabase simplifies the implementation of secure user registration, login, and session management (via JWTs), allowing the development team to focus on core application features. The .NET backend will be responsible for validating the JWTs provided by Supabase to enforce role-based access control.
