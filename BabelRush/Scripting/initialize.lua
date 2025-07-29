
luanet.load_assembly('BabelRush')
luanet.load_assembly('KirisameLib.Core')
luanet.load_assembly('KirisameLib.Logging')
luanet.load_assembly('GodotSharp')

cstype = luanet.import_type

-- temp code here
GD = cstype('Godot.GD')
local game = cstype('BabelRush.Game')
local log_level = cstype('KirisameLib.Logging.LogLevel')
local logger = game.LogBus:GetLogger('Lua');

-- 这个可以写个工具方法
GD.Print(luanet.ctype(game):IsInstanceOfType(game.Instance))


-- environment

local new_luanet = luanet -- todo: replace

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
	require = nil, -- todo
	-- modules:
	luanet = new_luanet,


	-- temp:
	GD = GD,
}

env._G = env

logger:Log(log_level.Info, 'Initialize', 'Lua frame loaded with version: ' .. env._VERSION)
