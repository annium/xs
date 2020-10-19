clean:
	@find . -type d -name bin | xargs rm -rf
	@find . -type d -name obj | xargs rm -rf

LINK_TARGET := ../backend

link:
	@./cli/scripts/link.js $(LINK_TARGET)

unlink:
	@./cli/scripts/unlink.js $(LINK_TARGET)

.PHONY: $(MAKECMDGOALS)