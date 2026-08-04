# Build Report

Status: Android release player, iOS Unity export, and unsigned iOS device build all succeeded from the final serialized project state.

| Platform | Configuration | Result | Unity errors/warnings | Output |
|---|---|---|---|---|
| Android | Release, IL2CPP, ARM64, adaptive icon, landscape | PASS — 24.01 s final cached build | 0 / 0 | `Builds/Android/ShadowTileEscape.apk` |
| iOS Unity export | Release, ARM64 Xcode project, iOS 15 minimum | PASS — 45.15 s | 0 / 0 | `Builds/iOS/Unity-iPhone.xcodeproj` |
| iOS Xcode compile | Release, iphoneos 26.5, signing disabled | PASS — `** BUILD SUCCEEDED **` | Unity/Xcode generated deprecation/linker warnings only | `Builds/iOSDerivedData/Build/Products/Release-iphoneos/ShadowTileEscape.app` |

## Artifact checks

- Android APK: 39,259,482 bytes (about 37.4 MiB); SHA-256 `f29a2ca651d8f254c1af8c69e5a0dbc9fd8052e805228b536fbd6fd1f5f08af5`.
- iOS export: about 1.0 GB across 3,112 source/project files; project SHA-256 `79d3e35f74a016f6c68815635e4c88fd5630c0fd38cc32d771ab71bc8c704d58`.
- Current Android application ID is `com.moonlitloom.shadowtileescape`; the user-updated iOS bundle ID is `com.moonlitsicku.shadowtileescape`. The iOS deployment target remains 15.0.
- The refreshed unsigned native compile used Xcode 26.6 / iphoneos 26.5 with signing disabled and ended `** BUILD SUCCEEDED **`.
- An initial Android UI-regression build reported five stale Sprite-rectangle warnings on overwritten QA screenshots. Those evidence assets were changed to ordinary Texture importers; the repeated final Android build and iOS export both completed with 0 errors/0 warnings.

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
