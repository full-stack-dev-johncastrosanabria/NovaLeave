namespace NovaLeave.Web.Filters;

/// <summary>
/// Marks a POST action that renders a user-facing form, so <see cref="ModelStateValidationFilter"/>
/// lets the action run and redisplay its view with the errors attached to the offending fields,
/// instead of short-circuiting with a machine-readable payload.
/// </summary>
/// <remarks>
/// Required by the constitution: MVC validation failures return the same view with
/// <c>ModelState</c> errors and MUST NOT be converted into generic responses (§11.4), and those
/// errors must be specific, actionable, and preserve non-sensitive input when the form is
/// redisplayed (§11.3).
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public sealed class RedisplayFormOnInvalidModelAttribute : Attribute;
