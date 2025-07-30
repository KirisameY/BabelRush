
local function sandbox_load(code, env, modenv, name)
	if (modenv==nil) then
		modenv = {}
	end
	if (modenv._metatable == nil) then
		setmetatable(modenv, {
			__index = function(t, k)
				return env[k]
			end
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
