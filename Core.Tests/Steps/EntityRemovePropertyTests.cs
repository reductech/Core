﻿using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class EntityRemovePropertyTests : StepTestBase<EntityRemoveProperty, Entity>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Remove property",
                new EntityRemoveProperty()
                {
                    Entity   = Constant(Entity.Create(("a", 1), ("b", 2))),
                    Property = Constant("b")
                },
                Entity.Create(("a", 1))
            );

            yield return new StepCase(
                "Remove missing property",
                new EntityRemoveProperty()
                {
                    Entity = Constant(Entity.Create(("a", 1))), Property = Constant("b")
                },
                Entity.Create(("a", 1))
            );

            yield return new StepCase(
                "Remove only property",
                new EntityRemoveProperty()
                {
                    Entity = Constant(Entity.Create(("b", 2))), Property = Constant("b")
                },
                Entity.Create()
            );
        }
    }
}

}
