# Build Report

Status: Android release player, iOS Unity export, and unsigned iOS device build all succeeded from the final serialized project state.

| Platform | Configuration | Result | Unity errors/warnings | Output |
|---|---|---|---|---|
| Android | Release, IL2CPP, ARM64, adaptive icon, landscape | PASS — 22.30 s final cached build | 0 / 0 | `Builds/Android/ShadowTileEscape.apk` |
| iOS Unity export | Release, ARM64 Xcode project, iOS 15 minimum | PASS — 43.66 s | 0 / 0 | `Builds/iOS/Unity-iPhone.xcodeproj` |
| iOS Xcode compile | Release, iphoneos 26.5, signing disabled | PASS — `** BUILD SUCCEEDED **` | Unity/Xcode generated deprecation/linker warnings only | `Builds/iOSDerivedData/Build/Products/Release-iphoneos/ShadowTileEscape.app` |

## Artifact checks

- Android APK: 37 MB; SHA-256 `5772c92ce7ca01d9d31afd24e4b684cf0298d35164260a551d1dce705351c923`.
- iOS export: about 1.0 GB across 3,112 source/project files; project SHA-256 `79d3e35f74a016f6c68815635e4c88fd5630c0fd38cc32d771ab71bc8c704d58`.
- iOS project contains bundle ID `com.moonlitloom.shadowtileescape` and `IPHONEOS_DEPLOYMENT_TARGET = 15.0`.
- The first sandboxed `xcodebuild` attempt failed because Bee could not bind its local IPC socket. The identical approved unsandboxed command passed; this was an environment restriction, not a source/build defect.

## Runtime profile

Unity Play Mode Level 1, profiler enabled without deep profiling:

- 7 batches / 7 draw calls / 7 set-pass calls.
- 465 triangles / 931 vertices.
- Sampled total frame: 16.512–16.927 ms (59.1–60.6 FPS in Editor).
- Sampled hotspots reported `0 B` GC allocation.
- Profiler analyzer returned zero optimization suggestions.

This is editor-side evidence, not a physical iPhone 8 thermal soak. Exact iPhone 8 layout was rendered at 1334×750; physical-device thermals, haptics, and store signing remain release-operator checks.

## Release-only inputs

- Android store signing requires a private keystore and passwords; none are committed.
- App Store archive/IPA requires an Apple team, distribution certificate, and provisioning profile.
- Analytics submission, engine diagnostics, and crash-report API are disabled in project settings.
