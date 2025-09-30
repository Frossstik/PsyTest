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
    .WaitFor(dependency: identityDb)
    .WithReference(rabbit);

// Main Service
var mainService = builder.AddProject<Projects.Psytest_ServiceMain>("mainservice")
    .WithReference(mainDb)
    .WaitFor(dependency: mainDb)
    .WithReference(identityService) // main «знает» об identity
    .WithReference(rabbit);

// Payment Service
//var paymentService = builder.AddProject<Projects.PaymentService>("paymentservice")
//    .WithReference(rabbit);

// Фронтенд (React)
var frontend = builder.AddNpmApp("frontend", "../psytest.frontend")
    .WithReference(mainService)
    .WithReference(identityService)
    .WaitFor(mainService)
    .WaitFor(identityService)
    .WithEnvironment("BROWSER", "none")
    .WithHttpEndpoint(env: "VITE_PORT") // Aspire динамически назначит порт и подставит в VITE_PORT
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
