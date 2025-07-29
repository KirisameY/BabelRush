local function sandbox_load(code, env)
	local chunk, err = load(code, 'sandboxed_chunk', 't', env)
	return chunk, err
end

return sandbox_load
