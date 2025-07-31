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
-- GD.Print(luanet.ctype(game):IsInstanceOfType(game.Instance))


-- environment

local processed_tables = setmetatable({}, { __mode = "k" })

local function make_readonly(tbl)
    if type(tbl) ~= 'table' then
        return tbl
    end

    -- 防止重复处理
    local proxy = processed_tables[tbl]
    if proxy ~= nil then
        return proxy-- 直接返回之前创建的代理
    end

    proxy = setmetatable({},{
    	__index = function(_t, k)
			local result = tbl[k]
			return make_readonly(result)
		end,

		__newindex = function(_t, k, _v)
			error("Attempt to modify a read-only table: " .. tostring(k), 2)
		end,

		__metatable = "protected",
    })

	-- 加入缓存并返回
	processed_tables[tbl] = proxy
    return proxy
end

local env = {
    _VERSION = _VERSION .. ' with BabelRush ' .. game.Version,

    -- original api:
    -- functions:
    assert = assert,
    collectgarbage = collectgarbage,
    error = error,
    getmetatable = getmetatable,
    setmetatable = setmetatable,
    ipairs = ipairs,
    --load = load,
    next = next,
    pairs = pairs,
    pcall = pcall,
    xpcall = xpcall,
    --rawequal = rawequal,
    --rawget = rawget,
    --rawlen = rawlen,
    --rawset = rawset,
    select = select,
    tonumber = tonumber,
    tostring = tostring,
    type = type,
    -- modules:
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

local readonly_env = make_readonly(env)
--env._G = make_readonly(env)

logger:Log(log_level.Info, 'Initialize', 'Lua frame loaded with version: ' .. env._VERSION)

return readonly_env
