
local function sandbox_load(code, env, modenv, name)
	if (modenv==nil) then
		modenv = {}
	end
	if (getmetatable(modenv) == nil) then
		setmetatable(modenv, {
			__index = function(_t, k)
				return env[k]
			end,

			__metatable = "protected",
		})
	end
	modenv._G = modenv

	if (name==nil) then
		name = 'unnamed_chunk'
	end

	local chunk, err = load(code, name, 't', modenv)
	return chunk, err
end

return sandbox_load
