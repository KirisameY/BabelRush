local envs = {}

local function get_env(namespace)
    local env = envs[namespace]
    if env then
        return env
    end

    env = {
        _NAMESPACE = namespace,
        import = function(id)
            local module = BabelRush.get_module(namespace, id)
            if module == nil then
                error('module \"' .. id .. '\" (with default namespace: \"' .. namespace .. '\") not found.')
            end
            return module
        end
    }
    envs[namespace] = env -- 加入缓存
    return env
end

return get_env
