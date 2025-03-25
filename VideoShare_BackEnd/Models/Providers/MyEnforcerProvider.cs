using Casbin;
using Casbin.AspNetCore.Authorization;
using Casbin.Persist.Adapter.EFCore;

namespace VideoShare_BackEnd.Models.Providers;

public class MyEnforcerProvider: IEnforcerProvider
{
    private readonly ICasbinModelProvider _modelProvider;
    private readonly CasbinDbContext<long> _casbinDbContext;
    private Enforcer _enforcer;

    public MyEnforcerProvider(ICasbinModelProvider modelProvider, CasbinDbContext<long> casbinDbContext)
    {
        _modelProvider = modelProvider;
        _casbinDbContext = casbinDbContext;
    }

    public IEnforcer? GetEnforcer()
    {
        _enforcer ??= new Enforcer(_modelProvider.GetModel(), new EFCoreAdapter<long>(_casbinDbContext));
        return _enforcer;
    }
}

