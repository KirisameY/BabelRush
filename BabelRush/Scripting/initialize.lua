
luanet.load_assembly('BabelRush')
luanet.load_assembly('KirisameLib.Core')
luanet.load_assembly('KirisameLib.Logging')
luanet.load_assembly('GodotSharp', 'Godot')

-- 这里这样替换还是太低效了，回头就像下面说的直接把整个Global替换掉
import = nil -- function () end
luanet.load_assembly = nil
cstype = luanet.import_type

-- temp code here
GD = cstype('Godot.GD')
local game = cstype('BabelRush.Game')
local log_level = cstype('KirisameLib.Logging.LogLevel')
game.LogBus:GetLogger('Lua'):Log(log_level.Info, 'Initialize', 'Lua frame loaded')

-- 这个可以写个工具方法
-- GD.Print(luanet.ctype(game):IsInstanceOfType(game.Instance))

-- todo: 将Global空间整个替换掉，然后返回原本的Global空间里的东西用于后续的初始化（如加载更多程序集）
