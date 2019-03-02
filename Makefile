clean:
	@find . -type d -name bin | xargs rm -rf
	@find . -type d -name obj | xargs rm -rf

.PHONY: $(MAKECMDGOALS)