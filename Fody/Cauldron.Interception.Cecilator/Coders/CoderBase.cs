﻿
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace Cauldron.Interception.Cecilator.Coders
{
    public abstract class CoderBase
    {
        internal readonly InstructionBlock instructions;

        protected CoderBase(InstructionBlock instructionBlock) => this.instructions = instructionBlock;

        public void InstructionDebug() => this.instructions.associatedMethod.Log(LogTypes.Info, this.instructions);

        public override string ToString() => this.instructions.associatedMethod.Fullname;

        protected object[] CreateParameters(Func<Coder, object>[] parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            return parameters.Select(x => x(this.instructions.associatedMethod.NewCoder())).ToArray();
        }

        protected void InternalCall(object instance, Method method) =>
            InternalCall(instance, method, new object[0]);

        protected void InternalCall(object instance, Method method, object[] parameters) =>
            this.instructions.Append(InstructionBlock.Call(this.instructions, instance, method, parameters));

        protected void NewObj(CustomAttribute attribute)
        {
            foreach (var arg in attribute.ConstructorArguments)
                this.instructions.Append(InstructionBlock.AttributeParameterToOpCode(this.instructions, arg));

            this.instructions.Emit(OpCodes.Newobj, attribute.Constructor);
        }

        #region Binary Operators

        protected void And(BuilderType currentType, Func<Coder, object> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            var value = other(this.NewCoder());

            if (InstructionBlock.AddBinaryOperation(this.instructions, OpCodes.And, currentType, null, value))
                return;

            this.instructions.Append(InstructionBlock.CreateCode(this.instructions, currentType, value));
            this.instructions.Emit(OpCodes.And);
        }

        protected void And(BuilderType currentType, Field field)
        {
            if (InstructionBlock.AddBinaryOperation(this.instructions, OpCodes.And, currentType, field.FieldType, field))
                return;

            this.instructions.Append(InstructionBlock.CreateCode(this.instructions, currentType, field));
            this.instructions.Emit(OpCodes.And);
        }

        protected void And(BuilderType currentType, LocalVariable variable)
        {
            if (InstructionBlock.AddBinaryOperation(this.instructions, OpCodes.And, currentType, variable.Type, variable))
                return;

            this.instructions.Append(InstructionBlock.CreateCode(this.instructions, currentType, variable));
            this.instructions.Emit(OpCodes.And);
        }

        protected void And(BuilderType currentType, ParametersCodeBlock arg)
        {
            var argInfo = arg.GetTargetType(this.instructions.associatedMethod);

            if (InstructionBlock.AddBinaryOperation(this.instructions, OpCodes.And, currentType, argInfo.Item1, arg))
                return;

            this.instructions.Append(InstructionBlock.CreateCode(this.instructions, currentType, arg));
            this.instructions.Emit(OpCodes.And);
        }

        protected void InvertInternal()
        {
            this.instructions.Emit(OpCodes.Ldc_I4_0);
            this.instructions.Emit(OpCodes.Ceq);
        }

        protected void Or(BuilderType currentType, Field field)
        {
            if (InstructionBlock.AddBinaryOperation(this.instructions, OpCodes.Or, currentType, field.FieldType, field))
                return;

            this.instructions.Append(InstructionBlock.CreateCode(this.instructions, currentType, field));
            this.instructions.Emit(OpCodes.Or);
        }

        protected void Or(BuilderType currentType, LocalVariable variable)
        {
            if (InstructionBlock.AddBinaryOperation(this.instructions, OpCodes.Or, currentType, variable.Type, variable))
                return;

            this.instructions.Append(InstructionBlock.CreateCode(this.instructions, currentType, variable));
            this.instructions.Emit(OpCodes.Or);
        }

        protected void Or(BuilderType currentType, Func<Coder, object> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            var value = other(this.NewCoder());

            if (InstructionBlock.AddBinaryOperation(this.instructions, OpCodes.Or, currentType, null, value))
                return;

            this.instructions.Append(InstructionBlock.CreateCode(this.instructions, currentType, value));
            this.instructions.Emit(OpCodes.Or);
        }

        protected void Or(BuilderType currentType, ParametersCodeBlock arg)
        {
            var argInfo = arg.GetTargetType(this.instructions.associatedMethod);

            if (InstructionBlock.AddBinaryOperation(this.instructions, OpCodes.Or, currentType, argInfo.Item1, arg))
                return;

            this.instructions.Append(InstructionBlock.CreateCode(this.instructions, currentType, arg));
            this.instructions.Emit(OpCodes.Or);
        }

        #endregion Binary Operators
    }

    public abstract class CoderBase<TSelf, TMaster> : CoderBase,
        IMathOperators<TSelf>
        where TSelf : CoderBase<TSelf, TMaster>
        where TMaster : CoderBase
    {
        protected CoderBase(InstructionBlock instructionBlock) : base(instructionBlock)
        {
        }

        public Method AssociatedMethod => this.instructions.associatedMethod;
        public abstract TMaster End { get; }

        public TSelf Append(TSelf coder)
        {
            this.instructions.Append(instructions);
            return this as TSelf;
        }

        public TSelf Append(InstructionBlock instructionBlock)
        {
            this.instructions.Append(instructionBlock);
            return this as TSelf;
        }

        public TSelf ArrayElement(int index)
        {
            this.instructions.Append(InstructionBlock.CreateCode(this.instructions, null, index));
            this.instructions.Emit(OpCodes.Ldelem_Ref);

            return this as TSelf;
        }

        /// <summary>
        /// Duplicates the last value in the stack
        /// </summary>
        /// <returns></returns>
        public TSelf Duplicate()
        {
            this.instructions.Emit(OpCodes.Dup);
            return (TSelf)this;
        }

        public TSelf Jump(Position position)
        {
            if (this.instructions.associatedMethod.IsInclosedInHandlers(position.instruction))
                this.instructions.Emit(OpCodes.Leave, position.instruction);
            else
                this.instructions.Emit(OpCodes.Br, position.instruction);

            return (TSelf)this;
        }

        /// <summary>
        /// Negates a value e.g. 1 to -1
        /// </summary>
        /// <returns></returns>
        public TSelf Negate()
        {
            this.instructions.Emit(OpCodes.Neg);
            return (TSelf)this;
        }

        public TSelf Pop()
        {
            this.instructions.Emit(OpCodes.Pop);
            return (TSelf)this;
        }

        protected void StoreElementInternal(BuilderType arrayType, object element, int index)
        {
            this.instructions.Append(InstructionBlock.CreateCode(this.instructions, null, index));
            this.instructions.Append(InstructionBlock.CreateCode(this.instructions, arrayType, element));
            this.instructions.Emit(OpCodes.Stelem_Ref);
        }
    }
}