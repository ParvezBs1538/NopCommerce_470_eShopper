using System;
using System.Linq.Expressions;
using FluentMigrator;
using FluentMigrator.Builders.Delete;
using Nop.Core;
using Nop.Data.Mapping;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart.Infrastructure;

public static class Extensions
{
    public static bool ShouldHaveValues(this FormAttributeMapping formAttributeMapping)
    {
        if (formAttributeMapping == null)
            return false;

        if (formAttributeMapping.AttributeControlType == AttributeControlType.TextBox ||
            formAttributeMapping.AttributeControlType == AttributeControlType.MultilineTextbox ||
            formAttributeMapping.AttributeControlType == AttributeControlType.Datepicker ||
            formAttributeMapping.AttributeControlType == AttributeControlType.FileUpload)
            return false;

        return true;
    }

    public static bool CanBeUsedAsCondition(this FormAttributeMapping formAttributeMapping)
    {
        if (formAttributeMapping == null)
            return false;

        if (formAttributeMapping.AttributeControlType == AttributeControlType.ReadonlyCheckboxes ||
            formAttributeMapping.AttributeControlType == AttributeControlType.TextBox ||
            formAttributeMapping.AttributeControlType == AttributeControlType.MultilineTextbox ||
            formAttributeMapping.AttributeControlType == AttributeControlType.Datepicker ||
            formAttributeMapping.AttributeControlType == AttributeControlType.FileUpload)
            return false;

        //other attribute control types support it
        return true;
    }

    public static bool ValidationRulesAllowed(this FormAttributeMapping formAttributeMapping)
    {
        if (formAttributeMapping == null)
            return false;

        if (formAttributeMapping.AttributeControlType == AttributeControlType.TextBox ||
            formAttributeMapping.AttributeControlType == AttributeControlType.MultilineTextbox ||
            formAttributeMapping.AttributeControlType == AttributeControlType.Datepicker ||
            formAttributeMapping.AttributeControlType == AttributeControlType.FileUpload)
            return true;

        //other attribute control types does not have validation
        return false;
    }

    public static bool IsNonCombinable(this FormAttributeMapping formAttributeMapping)
    {
        //When you have a form with several attributes it may well be that some are combinable,
        //whose combination may form a new SKU with its own inventory,
        //and some non-combinable are more used to add accessories

        if (formAttributeMapping == null)
            return false;

        //we can add a new property to "FormAttributeMapping" entity indicating whether it's combinable/non-combinable
        //but we assume that attributes
        //which cannot have values (any value can be entered by a customer)
        //are non-combinable
        var result = !ShouldHaveValues(formAttributeMapping);
        return result;
    }

    public static bool TableExists<TEntity>(this MigrationBase @base) where TEntity : BaseEntity
    {
        return @base.Schema.Table(NameCompatibilityManager.GetTableName(typeof(TEntity))).Exists();
    }

    public static void TableFor<TEntity>(this IDeleteExpressionRoot deleteExpression) where TEntity : BaseEntity
    {
        deleteExpression.Table(NameCompatibilityManager.GetTableName(typeof(TEntity)));
    }

    public static bool ColumnExists<TEntity>(this MigrationBase @base, Expression<Func<TEntity, object>> expression) where TEntity : BaseEntity
    {
        var columnName = NameCompatibilityManager.GetColumnName(
            typeof(TEntity),
            expression.Body is UnaryExpression unaryExpression ?
                (unaryExpression.Operand as MemberExpression).Member.Name :
                expression.Body is MemberExpression memberExpression ?
                memberExpression.Member.Name : null);

        if (string.IsNullOrEmpty(columnName))
            throw new Exception($"Wrong expression: {expression}");

        return @base.Schema.Table(NameCompatibilityManager.GetTableName(typeof(TEntity))).Column(columnName).Exists();
    }
}
