# 瞑想タイマーアプリ：作業手順書

このドキュメントは、MVVM 移行・タイマー精度改善・完了時の視覚演出を反映するための、Visual Studio 上での具体的な操作手順をまとめたものです。上から順に実施してください。

---

## フェーズ 0：作業前の確認（5 分）

### 0-1. Visual Studio のバージョン確認
1. Visual Studio 2022 を起動する。
2. メニュー「ヘルプ」→「Microsoft Visual Studio のバージョン情報」を開く。
3. バージョンが **17.8 以上** であることを確認する。古い場合は Visual Studio Installer から更新する。

### 0-2. MAUI ワークロードの確認
1. Windows のスタートメニューから「Visual Studio Installer」を起動する。
2. インストール済みの Visual Studio 2022 の「変更」ボタンを押す。
3. 「**.NET マルチプラットフォーム アプリ UI 開発**」にチェックが入っていることを確認。入っていなければチェックを入れて「変更」で再インストール。
4. 「個別のコンポーネント」タブで「**Windows App SDK**」関連も入れておく。

### 0-3. 既存プロジェクトを開く
1. Visual Studio で「最近開いた項目」または「プロジェクト/ソリューションを開く」から、現在の瞑想タイマープロジェクトを開く。
2. 開けたら、ソリューションエクスプローラ（通常は右側）でプロジェクト名を確認する。プロジェクト名が `MeditationTimer` であれば、私が提供したコードの `namespace` がそのまま使える。
3. 名前が違う場合は、ファイル冒頭の `namespace MeditationTimer.*` と `MainPage.xaml` の `x:Class="MeditationTimer.MainPage"`、`xmlns:vm="clr-namespace:MeditationTimer.ViewModels"` を、自分のプロジェクト名に置換する必要がある（このあと一括置換する手順を案内する）。

---

## フェーズ 1：NuGet パッケージのインストール（3 分）

### 1-1. NuGet パッケージマネージャを開く
1. ソリューションエクスプローラで **プロジェクト名（.csproj が下にあるノード）を右クリック**する。
2. メニューから「**NuGet パッケージの管理**」を選ぶ。

### 1-2. CommunityToolkit.Mvvm をインストール
1. 開いたパッケージマネージャの上部タブで「**参照**」をクリック。
2. 検索ボックスに `CommunityToolkit.Mvvm` と入力する。
3. 検索結果の **「CommunityToolkit.Mvvm」（作者: Microsoft）** を選ぶ。
4. 右側のペインで最新バージョン（8.x 以上）を選択し「**インストール**」をクリック。
5. ライセンス確認ダイアログが出たら「**同意する**」を押す。
6. 出力ウィンドウに「正常にインストールされました」と表示されればOK。

### 1-3. インストール確認
1. ソリューションエクスプローラの「依存関係」→「パッケージ」を展開する。
2. `CommunityToolkit.Mvvm` が一覧にあれば成功。

---

## フェーズ 2：フォルダ作成（2 分）

### 2-1. Services フォルダを作成
1. ソリューションエクスプローラでプロジェクト名を右クリック。
2. 「**追加**」→「**新しいフォルダー**」を選ぶ。
3. フォルダ名を `Services` と入力して Enter。

### 2-2. ViewModels フォルダを作成
1. 同じ手順で、もう一つフォルダを作成。
2. 名前は `ViewModels`。

ソリューションエクスプローラのプロジェクト直下に `Services/` と `ViewModels/` の 2 つのフォルダが並んでいれば完了。

---

## フェーズ 3：ファイルの作成・置き換え（10 分）

私が作成した 6 つのファイルを Visual Studio 上のプロジェクトに反映する。**ファイルごとに「新規作成 → コードを貼る」または「既存ファイルを開いて中身を全置換」のいずれか**を行う。

### 3-1. CountdownTimerService.cs（新規）
1. `Services` フォルダを右クリック → 「**追加**」→「**クラス**」。
2. ダイアログで「**クラス**」を選び、名前を `CountdownTimerService.cs` にして「追加」。
3. 開いたファイルの内容をすべて削除（Ctrl+A → Delete）。
4. 私が提供した `CountdownTimerService.cs` の内容を全て貼り付けて、Ctrl+S で保存。

### 3-2. MainPageViewModel.cs（新規）
1. `ViewModels` フォルダを右クリック → 「**追加**」→「**クラス**」。
2. 名前を `MainPageViewModel.cs` にして「追加」。
3. 中身を全削除し、私が提供した `MainPageViewModel.cs` の内容を貼り付け → Ctrl+S。

### 3-3. MainPage.xaml（既存を置き換え）
1. ソリューションエクスプローラで `MainPage.xaml` をダブルクリック。
2. XAML エディタで Ctrl+A → Delete で中身をすべて削除。
3. 私が提供した `MainPage.xaml` の内容を貼り付け → Ctrl+S。

### 3-4. MainPage.xaml.cs（既存を置き換え）
1. `MainPage.xaml` の左の三角を展開し、`MainPage.xaml.cs` をダブルクリック。
2. 中身を全削除し、私が提供した `MainPage.xaml.cs` の内容を貼り付け → Ctrl+S。

### 3-5. MauiProgram.cs（既存を置き換え）
1. ソリューションエクスプローラで `MauiProgram.cs` を開く。
2. 中身を全削除し、私が提供した `MauiProgram.cs` の内容を貼り付け → Ctrl+S。

### 3-6. App.xaml.cs（既存を置き換え。AppShell 利用なら後述）
1. `App.xaml` を展開して `App.xaml.cs` を開く。
2. 中身を全削除し、私が提供した `App.xaml.cs` の内容を貼り付け → Ctrl+S。

---

## フェーズ 4：AppShell の処理（重要・3 分）

.NET MAUI の標準テンプレートには `AppShell.xaml` が含まれており、これと私の `App.xaml.cs`（`MainPage = services.GetRequiredService<MainPage>()`）は衝突する。どちらか一方に統一する必要がある。

**今回は学習用なので「AppShell を削除して、直接 MainPage を表示する」を推奨。**

### 4-1. AppShell を削除する手順
1. ソリューションエクスプローラで `AppShell.xaml` を右クリック → 「削除」 → 確認ダイアログで「OK」。
2. `AppShell.xaml.cs` も一緒に消える（残っていれば同様に削除）。
3. これで `App.xaml.cs` の `MainPage = services.GetRequiredService<MainPage>();` が有効に動く。

### 4-2.（参考）AppShell を残したい場合
1. `App.xaml.cs` を私のサンプルではなく、以下のように書き換える：
   ```csharp
   public App()
   {
       InitializeComponent();
       MainPage = new AppShell();
   }
   ```
2. さらに `AppShell.xaml` 内の `<ShellContent>` の `ContentTemplate` が `MainPage` を指しているか確認する。
3. ただし DI で MainPage を解決させるには Shell の初期化方法を変える必要があり、初心者には複雑なので **推奨はフェーズ 4-1**。

---

## フェーズ 5：プロジェクト名が異なる場合の置換（必要な場合のみ・3 分）

フェーズ 0-3 でプロジェクト名が `MeditationTimer` 以外だと確認した場合だけ実施。

### 5-1. 一括置換
1. メニュー「**編集**」→「**検索と置換**」→「**フォルダー内検索**」（または Ctrl+Shift+H）。
2. 「検索する文字列」に `MeditationTimer` と入力。
3. 「置換後の文字列」に自分のプロジェクト名を入力。
4. 「検索する場所」を「**現在のプロジェクト**」に設定。
5. 「すべて置換」を押す。
6. 確認ダイアログで「はい」を選ぶ。

注意：プロジェクト名と一致する単語が他のところに含まれていると誤置換する可能性がある。差分を確認したい場合は「次を検索」で 1 件ずつ確認することもできる。

---

## フェーズ 6：ビルドして動作確認（10 分）

### 6-1. ビルド
1. メニュー「**ビルド**」→「**ソリューションのビルド**」（または Ctrl+Shift+B）。
2. 下部の出力ウィンドウで「ビルド: 成功 1、失敗 0」を確認。
3. **エラーが出た場合**：
   - 「エラー一覧」ウィンドウを開く（「表示」→「エラー一覧」）。
   - 「○○が定義されていません」系のエラーが多ければ、ほぼ namespace ミスマッチ。フェーズ 5 の一括置換が完全か確認。
   - `CommunityToolkit.Mvvm` 関連のエラーなら、フェーズ 1 のインストール失敗。再度確認。

### 6-2. デバッグターゲットを Windows に設定
1. ツールバー中央付近のドロップダウンで「**Windows Machine**」を選ぶ。
2. 緑色の実行ボタン（▶）の表示が「Windows Machine」になっていることを確認。

### 6-3. 実行
1. F5 キーを押す（または緑の▶ボタンをクリック）。
2. 初回ビルドは時間がかかる。少し待つ。
3. ダークテーマの画面に `00:00` と「瞑想・睡眠タイマー」のタイトルが表示されればOK。

### 6-4. 動作確認シナリオ
順番に動作させ、想定どおりか確認する。

1. **プリセット**：「10分」ボタンを押す → 中央の表示が `10:00` に変わる。
2. **開始**：「開始」ボタンを押す → 1 秒ごとに `09:59 → 09:58 → ...` と減る。ボタンが「一時停止」に変わる。
3. **一時停止**：「一時停止」を押す → 減算が止まる。ボタンが「再開」に変わる。
4. **再開**：「再開」を押す → 続きから減算再開。ボタンが「一時停止」に戻る。
5. **リセット**：「リセット」を押す → 表示が `10:00` に戻る。ボタンが「開始」に戻る。
6. **20 分プリセット**：「20分」を押す → 表示が `20:00` に変わる。

### 6-5. 完了動作のテスト（短縮テスト）
10 分待つのは大変なので、一時的にコードを書き換えて確認する。

1. `ViewModels/MainPageViewModel.cs` を開く。
2. `SetPreset` メソッド内の `TimeSpan.FromMinutes(minutes)` を **`TimeSpan.FromSeconds(minutes)`** に一時変更。
3. Ctrl+S で保存。Visual Studio が「ホットリロード」を提案したら適用、しなければ実行を停止して再度 F5。
4. 「10分」ボタンを押す（実際は 10 秒として動作）。
5. 「開始」を押して 10 秒待つ。
6. **0:00 になった瞬間に、カウントダウンの文字色がアクセントブルー（淡い青）に変わり、その下に「✨ 完了しました」が表示される**ことを確認。
7. ボタンが「もう一度」に変わっていることを確認。
8. 「もう一度」を押すと、青色表示が解除されて再びカウントダウンが始まることを確認。
9. **確認後、`FromSeconds` を `FromMinutes` に戻す**ことを忘れずに！

---

## フェーズ 7：Git でコミット（5 分）

### 7-1. Visual Studio 内蔵ターミナルを開く
1. メニュー「**表示**」→「**ターミナル**」（または Ctrl+\` ）。
2. ターミナルが下部に開き、プロジェクトのルートディレクトリにいるはず。

### 7-2. 変更内容を確認
```bash
git status
```
変更されたファイルや新規追加ファイルが一覧表示される。期待される変更：
- modified: `MainPage.xaml`, `MainPage.xaml.cs`, `MauiProgram.cs`, `App.xaml.cs`, `*.csproj`
- new file: `Services/CountdownTimerService.cs`, `ViewModels/MainPageViewModel.cs`
- deleted: `AppShell.xaml`, `AppShell.xaml.cs`（フェーズ 4-1 を実施した場合）

### 7-3. ステージング
全てまとめてステージングする場合：
```bash
git add .
```
個別に追加したい場合：
```bash
git add Services/CountdownTimerService.cs
git add ViewModels/MainPageViewModel.cs
git add MainPage.xaml MainPage.xaml.cs
git add MauiProgram.cs App.xaml.cs
git add MeditationTimer.csproj
```

### 7-4. コミット
```bash
git commit -m "feat(timer): MVVM 化と精度・完了表示の改善

- CountdownTimerService を壁時計ベースに変更してドリフトを排除
- MainPage を Command バインディング化し code-behind を最小化
- MainPageViewModel を CommunityToolkit.Mvvm で実装
- 完了時にカウントダウンの色変化と '完了' ラベルを表示
- MauiProgram で Service / ViewModel / View を DI 登録"
```

### 7-5. リモートにプッシュ（GitHub などにリポジトリを作成済みの場合）
```bash
git push origin main
```
（ブランチ名が `master` の場合は `main` を `master` に置き換える）

---

## トラブルシューティング

### 起動時に白い画面のまま固まる
- `MauiProgram.cs` の DI 登録漏れの可能性。`MainPage`・`MainPageViewModel`・`CountdownTimerService` が全部登録されているか確認。

### XAML エラー「'MainPageViewModel' is not found in 'clr-namespace:...'」
- `MainPage.xaml` の `xmlns:vm` で指定している namespace と、`MainPageViewModel.cs` 冒頭の `namespace` が一致しているか確認。
- ビルド前に表示されるエラーで、Build すれば解消する場合もある。

### 「型または名前空間の名前 'ObservableObject' が見つかりません」
- フェーズ 1 の NuGet インストールが失敗している。再インストール。

### ボタンを押しても反応しない
- `MainPage.xaml` で `Command="{Binding ...Command}"` の綴りミスがないか、ViewModel のメソッド名（`SetPreset` → `SetPresetCommand` のように "Command" が付く）と一致しているか確認。

### 完了時に色が変わらない
- `MainPage.xaml` の `DataTrigger` の `Binding="{Binding IsCompleted}"` が正しいか、`MainPageViewModel` で `IsCompleted` プロパティが `[ObservableProperty]` 属性付きか確認。

---

## 完了後の次の選択肢

1. **ユニットテストを追加**：`CountdownTimerService` のテストプロジェクトを追加し、`IDispatcher` のフェイクで検証。
2. **BGM 再生機能**：`Plugin.Maui.Audio` を NuGet 追加して `IAudioService` を作る。
3. **Android で動作確認**：エミュレータか実機で起動し、`KeepScreenOn` や通知の検討に進む。

進めたい方向が決まったら、その分のコードと手順を案内します。
