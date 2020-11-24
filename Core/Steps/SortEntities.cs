﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Entity = Reductech.EDR.Core.Entities.Entity;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Reorder entities according to their property values
    /// </summary>
    public sealed class SortEntities : CompoundStep<EntityStream>
    {
        /// <inheritdoc />
        public override async Task<Result<EntityStream, IError>> Run(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            var sortAscendingResult = await SortAscending.Run(stateMonad, cancellationToken);
            if (sortAscendingResult.IsFailure) return sortAscendingResult.ConvertFailure<EntityStream>();

            var entityStreamResult = await EntityStream.Run(stateMonad, cancellationToken);
            if (entityStreamResult.IsFailure) return entityStreamResult.ConvertFailure<EntityStream>();

            if (stateMonad.VariableExists(VariableName.Entity))
                return new SingleError($"Variable {VariableName.Entity} was already set.", ErrorCode.ReservedVariableName, new StepErrorLocation(this));

            //evaluate the entity stream

            var entitiesResult = await entityStreamResult.Value.TryGetResultsAsync(cancellationToken);

            if (entitiesResult.IsFailure)
                return entitiesResult.ConvertFailure<EntityStream>();


            var list = new List<(string property, Entity entity)>();

            foreach (var entity in entitiesResult.Value)
            {
                var setResult = stateMonad.SetVariable(VariableName.Entity, entity);
                if (setResult.IsFailure) return setResult.ConvertFailure<EntityStream>();
                var propertyValue = await SortBy.Run(stateMonad, cancellationToken);
                if (propertyValue.IsFailure) return propertyValue.ConvertFailure<EntityStream>();

                list.Add((propertyValue.Value, entity));

            }
            stateMonad.RemoveVariable(VariableName.Entity, false);

            var sortedList =
                sortAscendingResult.Value ?
                    list.OrderBy(x => x.property) :
                    list.OrderByDescending(x => x.property);

            var resultsList = sortedList.Select(x => x.entity).ToList();

            var newStream = Entities.EntityStream.Create(resultsList);

            return newStream;
        }

        /// <summary>
        /// The entities to sort
        /// </summary>
        [StepProperty(Order = 1)]
        [Required]
        public IStep<EntityStream> EntityStream { get; set; } = null!;

        /// <summary>
        /// A function that gets the key to sort by from the variable &lt;Entity&gt;
        /// To sort by multiple properties, concatenate several keys
        /// </summary>
        [StepProperty(Order = 2)]
        [Required]
        public IStep<string> SortBy { get; set; } = null!;

        /// <summary>
        /// Whether to sort in ascending order.
        /// </summary>
        [StepProperty(Order = 3)]
        [DefaultValueExplanation("True")]
        public IStep<bool> SortAscending { get; set; } = new Constant<bool>(true);


        /// <inheritdoc />
        public override IStepFactory StepFactory => SortEntitiesStepFactory.Instance;
    }

    /// <summary>
    /// Reorder entities according to their property values
    /// </summary>
    public sealed class SortEntitiesStepFactory : SimpleStepFactory<SortEntities, EntityStream>
    {
        private SortEntitiesStepFactory() { }

        /// <inheritdoc />
        public override IEnumerable<(VariableName VariableName, ITypeReference typeReference)> FixedVariablesSet =>
            new (VariableName VariableName, ITypeReference typeReference)[]
            {
                (VariableName.Entity, new ActualTypeReference(typeof(Entity)))
            };

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<SortEntities, EntityStream> Instance { get; } = new SortEntitiesStepFactory();
    }

}
