# 인벤토리 및 화면 전환

`Assets/Scenes/SampleScene.unity`를 열고 Play를 누르면 UI가 생성됩니다.

- 왼쪽 아래 INVENTORY 버튼 또는 I: 인벤토리 열기·닫기
- 같은 버튼, Esc 또는 슬롯 바깥 클릭: 인벤토리 닫기
- 화면 좌우 가장자리의 삼각형 버튼 또는 키보드 좌우 화살표: 화면 전환
- 첫 화면의 이전 버튼과 마지막 화면의 다음 버튼은 비활성화됩니다.
- 인벤토리가 열려 있는 동안 화면 이동은 차단됩니다.

씬의 Room UI 오브젝트에서 RoomUIController의 Rooms 배열로 화면 제목, 설명, 배경색과 순서를 편집할 수 있습니다. Inventory Slots는 빈 슬롯 수입니다. 현재 슬롯은 표시용이며 아이템 수집·사용·저장은 포함하지 않습니다.

다른 게임 로직은 `Navigate`, `SetInventoryOpen`, `ToggleInventory`를 호출하거나 `RoomChanged`, `InventoryChanged` 이벤트를 구독할 수 있습니다. 화면 전환은 같은 씬 내 뷰 전환입니다.

UI 스타일은 `Assets/Resources/RoomUI.uss`에서 변경합니다. Unity UI Toolkit 및 Input System을 사용하며 추가 패키지 설치는 필요하지 않습니다.

참고 스케치에 맞춰 중앙을 비우고, 오른쪽 위에 상태 바 두 개와 원형 표시, 오른쪽 아래에 벡터 손 실루엣, 왼쪽 아래에 5열 인벤토리를 배치했습니다. 상태 바는 현재 표시용 샘플 값(체력 85%, 정신력 65%)이며 게임 상태와 연결되어 있지 않습니다. 손과 화살표는 `RoomUIController.cs`의 `SketchGraphic` 클래스에서 해상도에 맞게 그립니다.

