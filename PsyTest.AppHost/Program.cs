using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// База PostgreSQL
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .WithDataVolume();

// Добавляем базу для Identity
var identityDb = postgres.AddDatabase("identitydb");
// Добавляем базу для Main
var mainDb = postgres.AddDatabase("maindb");

// RabbitMQ
var rabbit = builder.AddRabbitMQ("rabbit");

// Identity Service
var identityService = builder.AddProject<Projects.PsyTest_ServiceIdentity>("identityservice")
    .WithReference(identityDb)
    .WithReference(rabbit);

// Main Service
var mainService = builder.AddProject<Projects.Psytest_ServiceMain>("mainservice")
    .WithReference(mainDb)
    .WithReference(identityService) // main «знает» об identity
    .WithReference(rabbit);

// Payment Service
//var paymentService = builder.AddProject<Projects.PaymentService>("paymentservice")
//    .WithReference(rabbit);

// Фронтенд (React)
var frontend = builder.AddNpmApp("frontend", "../psytest.frontend")
    .WithReference(mainService)
    .WithReference(identityService)
    //.WithReference(paymentService)
    .WithHttpEndpoint(env: "VITE_API_BASE");

builder.Build().Run();
