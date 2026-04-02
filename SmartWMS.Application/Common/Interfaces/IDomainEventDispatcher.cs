namespace SmartWMS.Application.Common.Interfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using SmartWMS.Domain.Common;

public interface IDomainEventDispatcher
{
    Task DispatchAndClearEvents(IEnumerable<AggregateRoot> entitiesWithEvents);
}
