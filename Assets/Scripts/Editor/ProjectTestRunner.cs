using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace ShadowTileEscape.Editor
{
    public static class ProjectTestRunner
    {
        const string ResultsPath = "TestResults/editmode-results.xml";
        const string PlayModeResultsPath = "TestResults/playmode-results.xml";

        [MenuItem("Shadow Tile Escape/QA/Run EditMode Tests")]
        public static void RunEditModeTests()
        {
            Run(TestMode.EditMode, "ShadowTileEscape.EditModeTests", ResultsPath);
            Debug.Log("[ShadowTileEscape QA] EditMode test run started.");
        }

        [MenuItem("Shadow Tile Escape/QA/Run PlayMode Tests")]
        public static void RunPlayModeTests()
        {
            Run(TestMode.PlayMode, "ShadowTileEscape.PlayModeTests", PlayModeResultsPath);
            Debug.Log("[ShadowTileEscape QA] PlayMode test run started.");
        }

        static void Run(TestMode mode, string assemblyName, string resultsPath)
        {
            Directory.CreateDirectory("TestResults");
            if (File.Exists(resultsPath)) File.Delete(resultsPath);
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(new ResultsCallback(resultsPath));
            api.Execute(new ExecutionSettings(new Filter { testMode = mode, assemblyNames = new[] { assemblyName } }));
        }

        sealed class ResultsCallback : ICallbacks
        {
            readonly string resultsPath;

            public ResultsCallback(string resultsPath) => this.resultsPath = resultsPath;
            public void RunStarted(ITestAdaptor testsToRun) { }
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }

            public void RunFinished(ITestResultAdaptor result)
            {
                TestRunnerApi.SaveResultToFile(result, resultsPath);
                var message = $"[ShadowTileEscape QA] Tests complete: {result.PassCount} passed, {result.FailCount} failed, {result.SkipCount} skipped.";
                if (result.FailCount == 0) Debug.Log(message); else Debug.LogError(message);
            }
        }
    }
}
