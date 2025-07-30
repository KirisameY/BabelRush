
local envs = {}

local function get_env(namespace)
	local env = envs[namespace]
	if env == nil then
		env = {
			_NAMESPACE = namespace,
			import = function(id)
			    local module = BabelRush.get_module(namespace, id)
				if module == nil then
					error('module \"'.. id .. '\" (with default namespace: \"' .. namespace .. '\") not found.')
				end
				return module
			end
		}
		envs[namespace] = env
	end
	return env
end

return get_env
