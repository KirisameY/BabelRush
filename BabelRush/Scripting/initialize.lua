
luanet.load_assembly('BabelRush')
luanet.load_assembly('KirisameLib.Core')
luanet.load_assembly('KirisameLib.Data')
luanet.load_assembly('KirisameLib.Event')
luanet.load_assembly('KirisameLib.Logging')
luanet.load_assembly('GodotSharp')

local cstype = luanet.import_type

-- temp code here
local GD = cstype('Godot.GD')
local game = cstype('BabelRush.Game')
local log_level = cstype('KirisameLib.Logging.LogLevel')
local logger = game.LogBus:GetLogger('Lua');

-- 这个可以写个工具方法
-- GD.Print(luanet.ctype(game):IsInstanceOfType(game.Instance))


-- environment

local env = {
	_VERSION = _VERSION .. ' with BabelRush ' .. game.Version,

	-- original api:
	-- functions:
	assert = assert,
	collectgarbage = collectgarbage,
	error = error,
	getmetatable = getmetatable,
	ipairs = ipairs,
	load = load,
	next = next,
	pairs = pairs,
	pcall = pcall,
	rawequal = rawequal,
	rawget = rawget,
	rawlen = rawlen,
	rawset = rawset,
	select = select,
	setmetatable = setmetatable,
	tonumber = tonumber,
	tostring = tostring,
	type = type,
	xpcall = xpcall,
	-- modules:
	debug = debug,
	math = math,
	string = string,
	table = table,
	utf8 = utf8,

	-- custom api:
	-- functions:
	cstype = luanet.import_type,
	-- modules:
	BabelRush = BabelRush, -- inject from C# side
	dotnet = {
		ctype = luanet.ctype,
		instanceof = function(obj, type)
			return luanet.ctype(type):IsInstanceOfType(obj)
		end
	},
}

-- env._G = env

logger:Log(log_level.Info, 'Initialize', 'Lua frame loaded with version: ' .. env._VERSION)

return env
