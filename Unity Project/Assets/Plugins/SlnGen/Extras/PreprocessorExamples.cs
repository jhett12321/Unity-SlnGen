namespace SlnGen.Extras
{
    using System.Diagnostics;
    using UnityEngine.Assertions;
    using Debug = UnityEngine.Debug;

    public class PreprocessorExamples
    {
        [Conditional("UNITY_EDITOR")]
        public void SomeEditorMethod()
        {
            Debug.Log("Some editor code");
        }

        public void SomeMixedMethod()
        {
            // Method is auto-excluded in players as it has a conditional editor attribute.
            SomeEditorMethod();

            // Do some assertions
            Assert.IsTrue(true, "Excluded in Player");
            Assert.IsNull(null, "But included in Debug/Editor");

            // Compile error - Editor code usage without preprocessor
            UnityEditor.EditorApplication.delayCall += () => Debug.Log("Bad-code for players!");

            // Display a message depending on our current target.
            #if UNITY_EDITOR
            Debug.Log("Some editor code");
            #elif DEBUG
            Debug.Log("Some development player code");
            #else
            Debug.Log("Some player code");
            #endif

            // Display a log message depending on our selected platform
            #if UNITY_STANDALONE
            Debug.Log("Standalone platform code");
            #elif UNITY_PS4
            Debug.Log("PS4 platform code");
            #elif UNITY_XBOXONE
            Debug.Log("Xbox platform code");
            #else
            Debug.Log("Some other platform code");
#endif
        }
    }
}