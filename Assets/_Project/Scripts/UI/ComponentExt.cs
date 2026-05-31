using UnityEngine;

namespace Saga.UI
{
    /// <summary>
    /// Sprint 9 — defensive Unity component helpers.
    ///
    /// Unity gotcha : <c>?? AddComponent</c> ne marche PAS comme attendu car
    /// <c>GetComponent&lt;T&gt;()</c> retourne un Unity-style "fake null" (UnityEngine.Object overload
    /// du operator==) qui n'est PAS un null référence C# pur. Le <c>??</c> opérateur utilise
    /// le null reference check pur → fallback jamais déclenché → MissingComponentException.
    ///
    /// Use <see cref="GetOrAdd"/> partout au lieu de <c>?? AddComponent</c>.
    /// </summary>
    public static class ComponentExt
    {
        /// <summary>
        /// Returns the component if attached, sinon en ajoute un nouveau.
        /// Safe vs Unity fake-null (utilise <c>==</c> qui appelle Unity Object overload).
        /// </summary>
        public static T GetOrAdd<T>(this GameObject go) where T : Component
        {
            var existing = go.GetComponent<T>();
            if (existing == null) existing = go.AddComponent<T>();
            return existing;
        }

        /// <summary>Overload pour partir d'un Component plutôt qu'un GameObject.</summary>
        public static T GetOrAdd<T>(this Component c) where T : Component
        {
            return c.gameObject.GetOrAdd<T>();
        }
    }
}
