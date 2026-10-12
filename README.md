# H5 可玩广告 Demo（Unity WebGL）

面向可玩广告（Playable Ad）场景的收集合成玩法 Demo，独立开发，可导出为网页在浏览器运行。

## 功能

- 收集合成玩法：物品飞入背包、库存计数、倒计时与洗牌重排
- 磁铁、弹弓、风扇、冻结四类道具（协程实现）
- 闪电球生成与命中特效、道具剩余次数 UI

## 技术要点

- 道具系统（ToolManager）+ 游戏主循环（GameManager）
- 背包 / 计数 / 计时 UI 模块化
- Unity WebGL 导出

## 说明

核心逻辑位于 `Scripts/`。
