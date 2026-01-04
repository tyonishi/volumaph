# VoluMaph プロジェクトルール

VoluMaph はディスク容量を高速に可視化する Windows アプリケーションです。

## プロジェクト概要

**アプリ名**: VoluMaph  
**目的**: PC 内のディスク使用量を高速にスキャンし、フォルダ構造を視覚的にマッピングして表示する  
**技術スタック**:
- C# / .NET 8
- UI: WinUI 3 または WPF
- ターゲット OS: Windows 10 / 11

## 設計ドキュメント

- **基本設計書**: `docs/design.md` - プロジェクト概要・機能要件・アーキテクチャ
- **詳細設計書**: `docs/detailed-design.md` - 各モジュールの詳細設計・コード例
- **MVP タスク**: `docs/plans/mvp-todo.md` - 開発タスク分解
- コーディングを行う際は必ず `docs/detailed-design.md` を参照してください

## プロジェクト構成

```
volumaph/
 ├─ src/
 │   ├─ VoluMaph.Core/          # コアロジック
 │   │   ├─ Scanner/           # スキャナー
 │   │   ├─ FileSystemModel/   # ファイルシステムモデル
 │   │   └─ Analyzer/          # 分析・集計
 │   ├─ VoluMaph.UI/            # UI レイヤー
 │   │   ├─ ViewModels/
 │   │   ├─ Views/
 │   │   └─ Controls/
 │   └─ VoluMaph.Infrastructure/# インフラストラクチャ
 │       ├─ Logging/
 │       ├─ Settings/
 │       └─ NativeInterop/
 ├─ docs/
 │   ├─ design.md
 │   ├─ detailed-design.md
 │   └─ plans/
 │       └─ mvp-todo.md
 ├─ tests/
 └── AGENTS.md                  # このファイル
```

## コーディング規則

### 命名規則
- **クラス名**: PascalCase (`DirectoryScanner`, `FileNode`)
- **メソッド名**: PascalCase (`Scan`, `Analyze`)
- **プロパティ名**: PascalCase (`FileSize`, `Percentage`)
- **フィールド名**: `_camelCase` (private), `PascalCase` (public/protected)
- **ローカル変数**: camelCase
- **定数**: PascalCase (`MaxFileSize`, `DefaultBufferSize`)

### アーキテクチャ原則
- **疎結合**: スキャンエンジンと UI を分離
- **疎結合**: スキャンエンジンと UI を疎結合にする
- **CQRS**: 読み取り（スキャン）と書き込み（UI 更新）を分離

### 非同期処理
- ファイルシステム操作は必ず非同期で (`Task`, `async`/`await`)
- UI 更新は必ず UI スレッドで実行
- `IProgress<T>` で進捗報告

### エラーハンドリング
- ユーザーに見せるエラーはローカライズされたメッセージ
- ログには詳細なスタックトレースとコンテキストを記録
- 権限不足などの予期される例外は適切に処理

## 開発フロー

### 新機能開発
1. `docs/detailed-design.md` を参照し、実装仕様を確認
2. `docs/plans/mvp-todo.md` で現在のフェーズとタスクを確認
3. TDD に従いテストを先に作成
4. 実装
5. テストを実行し確認
6. リンターと型チェックを実行

### コマンド
- **ビルド**: `dotnet build`
- **テスト**: `dotnet test`
- **リンター**: `dotnet format` またはプロジェクト固有のコマンド
- **型チェック**: `dotnet build` (C# はコンパイル時に型チェック)

## コードスタイル
- 不要なコメントは書かない
- コード自体で意図を明確にする
- ファイル末尾に改行を入れる
- 変数名・メソッド名は意図を明確に表現する

## 優先事項
1. パフォーマンス（高速スキャン）
2. シンプルな UI/UX
3. 拡張性
4. Windows 11 の Fluent Design との親和性

## C# Coding Guidelines (Best Practices)
- コーディング時は `docs/coding-guidelines.md` を確認し、命名規則・コードフォーマット・コメント方針を遵守する。
- `docs/best-practices.md` を併読し、例外処理・IDisposable/using パターン・非同期処理・API 設計に関する推奨事項を適用する。
