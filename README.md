# Unity アプリケーション制作総合実習
 
指導講師：小西秀明（職業訓練校 Unity講座）

---

## 目次

- [プロジェクト概要](#project-overview)
- [使用予定のUnity機能](#unity-features)
- [進行スケジュール（目安）](#schedule)
- [進捗メモ](#progress-notes)
- [開発ルール](#development-rules)
- [発表内容](#presentation-plan)
- [3Dモデルが正常に表示されない場合の解決策](#problem-solution)

---

## プロジェクト概要 <a id="project-overview"></a>

| 項目 | 内容 |
|------|------|
| **ゲームタイトル** | Painter of Light |
| **一言で説明すると** | 光のスフィアを展開して見えなかったオブジェクトやアイテムを見つける。取得したアイテムで自身強化し続けるを3Dアクションゲーム |
| **ゲームの目的** | HPが尽きるまで探索や戦闘を行いプレイヤーを強化しボスを討伐する |
| **プレイヤー操作** | WASDで移動、マウスで視点操作、左クリックで攻撃、右クリックで周囲を照らすアクション、Eキーで必殺技、Rキーで超必殺技、Fキーでインタラクト（ドアを開ける、宝箱を開けるなど）1、2，3キーで操作キャラクターのスワップ、Escキーでステータス確認及びメニュー画面 |
| **勝利条件** | ボスの討伐 |
| **失敗条件** | HPが0になる |
| **特徴・工夫点** | 光を照らすことで視界を確保する演出 |

---

## 使用予定のUnity機能 <a id="unity-features"></a>

☑ = 使用予定（下記チェックボックスに `[x]` を入れる）  

> 使用予定の機能は `[ ]` を `[x]` に変更してください。  
> 例：  
> `- [x] PlayerController（移動・入力）` → 使用する機能  
> `- [ ] NavMesh / AI` → 今回は使用しない機能

- [x] PlayerController（移動・入力）  
- [x] Trigger / Collider  
- [x] UI（Canvas, Button, TextMeshPro）  
- [x] ScriptableObject / JSONデータ  
- [x] アニメーション / DOTween  
- [x] サウンド（BGM・SE）  
- [x] NavMesh / AI  
- [x] Timeline / Cinemachine  
- [ ] その他（　　　　　　　　　　　　　　　　　　　　）

---

## 進行スケジュール（目安） <a id="schedule"></a>

| 日 | 内容 | 主な成果物 |
|----|------|------------|
| 1週目 | 企画書作成・Git初期化 | README.md完成、マップ、プレイヤーの配置 |
| 2週目 | プロトタイプ実装開始 | プレイヤー移動・探索判定、プレイテスト |
| 3週目 | UI・探索対象の追加 | 画面構成確定、デバック |
| 4週目 | ステージ・演出・サウンド統合 | デバッグ・最終調整、プレゼン＋作品公開 |


---


## 開発ルール <a id="development-rules"></a>

- **Git運用**  
  - 1〜2時間ごとにCommit  
  - 1日2〜3回Push  
  - 作業前に必ず「Pull」して最新を取得  
  - コンフリクトが起きた場合はリーダーが調整  

- **Unityプロジェクト設定**  
  - Unity 6（URP）を使用  
  - 各自、同バージョンを統一  
  - 不要アセットは追加しない（軽量管理）  

- **フォルダ構成推奨**
```
Assets/
├─ Scripts/
├─ Prefabs/
├─ Scenes/
├─ UI/
└─ Audio/
```
---

## 発表内容 <a id="presentation-plan"></a>

| 項目 | 内容 |
|------|------|
| **タイトル** | Painter of Light |
| **ゲーム紹介** | 音と光を頼りにアイテムを探し、取得したアイテムで自身強化し続けるを3Dアクションゲーム |
| **アピールポイント** | 光を照らすことで視界を確保する演出と、音によるヒント表示を組み合わせた誘導演出 |
| **頑張った点** | DOTweenを活用した演出とマップ、キャラクター、UIの見た目による世界観の統一 |

---

## 3Dモデルが正常に表示されない場合の解決策 <a id="#problem-solution"></a>

UnityのProjectウィンドウにて下記のフォルダを右クリックして、Reimport を選択してください。

・Assets/UniGLTF

・Assets/VRM10 （または VRM）

同様に3Dモデル（.vrm）も再インポートします。

Assets/1_VRoid/Lily.vrm を 右クリックし、Reimport を選択します。


上記の手順で表示されない場合は、GitからLFSの設定を行います。

GitHub Desktopなどからメニューバーの 「Repository」 > 「Show in Explorer」 をクリックし、コマンドプロンプトを開きます。
Gitがインストールされていない場合は、Unable to local Gitと表示されているポップアップから、Install Gitボタンを押してダウンロードしてください。

コマンドプロンプトが起動できて、プロジェクトフォルダを開けている場合は、下記の手順でLFSの実データをダウンロードします。

1.LFS が有効か確認します。
git lfs install

2.最新の変更をプルします。
git pull

3.LFSの実データをダウンロードします。
git lfs pull

4.それでもなおUnable to local Gitと表示される場合は、PCを再起動を行ってください。インストールしたGitがPCに認識されていない状態が解消されるかもしれません。

5.再度、UniGLTF、VRM10、3Dモデル（.vrm）のReimportを行ってください。

---



© 2025 職業訓練校 Unity講座（講師：小西秀明）
