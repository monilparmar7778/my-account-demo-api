var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.my_account_api>("my-account-api");

builder.Build().Run();
