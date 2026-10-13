# 인벤토리 및 화면 전환

`Assets/Scenes/SampleScene.unity`를 열고 Play를 누르면 건물 영상 → 침대에서 일어나는 영상(초인종 포함) → 주인공 방 이미지와 UI 순서로 시작합니다. 외부에서 씬 파일을 수정한 뒤 Unity에서 이미 열어 둔 씬에 반영되지 않으면 Play를 종료하고 SampleScene을 다시 열어 주세요.

인트로 동안 Room UI 오브젝트는 비활성 상태이며, Intro Sequence 오브젝트의 `IntroSequenceController`가 두 영상의 종료 이벤트를 받아 차례로 재생한 후 Room UI를 활성화합니다. 영상의 원본 소리는 그대로 재생합니다. 원본 비율을 유지하므로 화면 비율이 다르면 여백이 생길 수 있습니다. 영상 준비가 실패하거나 30초를 넘으면 Console에 오류를 남기고 다음 단계로 진행합니다.

- 건물 영상: `Assets/StreamingAssets/Intro/Building.mp4`
- 침대 영상: `Assets/StreamingAssets/Intro/WakingUp.mp4`
- 주인공 방 배경: `Assets/Images/ProtagonistRoom.png`

영상은 StreamingAssets에 포함되므로 Windows 빌드에도 함께 들어갑니다. Intro Sequence의 Video Files 배열 순서로 재생 순서를 바꿀 수 있습니다. 게임 시작 씬은 Build Settings에도 SampleScene으로 지정되어 있습니다.

- 왼쪽 아래 INVENTORY 버튼 또는 I: 인벤토리 열기·닫기
- 같은 버튼, Esc 또는 슬롯 바깥 클릭: 인벤토리 닫기
- 화면 좌우 가장자리의 삼각형 버튼 또는 키보드 좌우 화살표: 화면 전환
- 첫 화면의 이전 버튼과 마지막 화면의 다음 버튼은 비활성화됩니다.
- 인벤토리가 열려 있는 동안 화면 이동은 차단됩니다.

주인공 방은 Rooms의 Allow Navigation을 꺼 두었습니다. 이 장소에서는 좌우 화살표가 숨겨지고 좌우 방향키 및 `Navigate` 호출에 의한 이동이 차단됩니다. 인벤토리는 그대로 사용할 수 있습니다. 다른 장소는 Allow Navigation을 켜면 기존 화면 전환 기능을 사용합니다.

씬의 Room UI 오브젝트에서 RoomUIController의 Rooms 배열로 장소 데이터, 배경색, Image와 순서를 편집할 수 있습니다. 제목과 설명은 화면에 표시하지 않습니다. Image가 있으면 원본 비율을 유지하며 화면에 맞춰 표시하고, 없으면 배경색을 표시합니다. 첫 번째 방에 주인공 방 이미지가 연결되어 있으며 나머지 방은 기존 샘플입니다. Inventory Slots는 빈 슬롯 수입니다. 현재 슬롯은 표시용이며 아이템 수집·사용·저장은 포함하지 않습니다.

다른 게임 로직은 `Navigate`, `SetInventoryOpen`, `ToggleInventory`를 호출하거나 `RoomChanged`, `InventoryChanged` 이벤트를 구독할 수 있습니다. 화면 전환은 같은 씬 내 뷰 전환입니다.

UI 스타일은 `Assets/Resources/RoomUI.uss`에서 변경합니다. Unity UI Toolkit 및 Input System을 사용하며 추가 패키지 설치는 필요하지 않습니다.

왼쪽 아래에 5열 인벤토리를 배치했습니다. 방 이미지 위에서도 읽을 수 있도록 인벤토리 버튼과 이동 화살표에 어두운 반투명 배경을 적용했습니다. 좌측 상단의 장소 정보창(제목·설명·화면 번호)과 우측 상단의 게이지 및 원형 표시는 공통 UI에서 완전히 제거했습니다. 샘플 손 실루엣은 이 씬에서 Show Hand를 꺼서 숨겼으며 Inspector에서 다시 켤 수 있습니다. 손과 화살표는 `RoomUIController.cs`의 `SketchGraphic` 클래스에서 해상도에 맞게 그립니다.

