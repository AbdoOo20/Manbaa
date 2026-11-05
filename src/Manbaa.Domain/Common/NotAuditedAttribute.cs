namespace Manbaa.Domain.Common;

/// <summary>Excludes an entity from the automatic audit trail. Use it only for records that are themselves logs.</summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class NotAuditedAttribute : Attribute;
