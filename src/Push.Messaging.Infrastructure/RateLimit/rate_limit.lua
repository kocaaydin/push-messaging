-- KEYS[1] = rate key
-- ARGV[1] = limit
-- ARGV[2] = ttl (seconds)

if redis.call("EXISTS", KEYS[1]) == 0 then
  redis.call("SET", KEYS[1], ARGV[1], "EX", ARGV[2])
end

local remaining = redis.call("DECR", KEYS[1])
if remaining < 0 then
  return 0
end

return 1