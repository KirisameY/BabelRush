
local function sandbox_load(code, env, modenv)
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

	local chunk, err = load(code, 'sandboxed_chunk', 't', modenv)
	return chunk, err
end

return sandbox_load
