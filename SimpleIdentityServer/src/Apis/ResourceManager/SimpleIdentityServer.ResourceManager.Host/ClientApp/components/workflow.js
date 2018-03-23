var d3 = require("d3");
import $ from 'jquery';
import React, { Component } from "react";

const defaultOpts = {
	frame: {
		width: 'auto',
		height: 600
	},
	cell: {
		leftPadding: 50,
		topPadding: 30,
		leftTextPadding: 5,
		topTextPadding: 5,
		width: 130,
		height: 50,
		fontSize: '15px',
		click: null
	},
	data: []
};

class Workflow extends Component {
	constructor(props) {
		super(props);
	}

	render() {
		return (<div ref="main"></div>);
	}

	show(opts) {
		$(this.refs.main).empty();
		this.options = $.extend(true, {}, defaultOpts, opts);
		var nbCols = 1;
		var self = this;
		function getRectX(col) {
			if (col === 0) {
				return 10;
			}

			return  col * (self.options.cell.leftPadding + self.options.cell.width);
		}

		function getRectY(elts, d) {
			var index = elts.indexOf(d);
			var totalSize = elts.length * (self.options.cell.height + self.options.cell.topPadding);
			var halfSize = totalSize / 2;
			var minY = (self.options.frame.height / 2) - halfSize;
			return minY + (index) * (self.options.cell.topPadding + self.options.cell.height);
		}

		function drawElts(elts, col) {
			var newCol = col + 1;
			elts.forEach(function(elt) {
				if (elt.sub && elt.sub.length > 0) {
					var g = chart
						.selectAll('line_' + elts.indexOf(elt) + '_'+col)
						.data(elt.sub).enter().append('g');
					var x1 = getRectX(col) + self.options.cell.width;
					var y1= getRectY(elts, elt) + self.options.cell.height / 2;
					var x2 = getRectX(newCol) - 10;
					g.append('line').style('stroke', 'black')
						.attr('x1', function(d) { return x1; })
						.attr('y1', function(d) { return y1; })
						.attr('x2', function(d) { return x2; })
						.attr('marker-end', 'url(#marker)')
						.attr('y2', function(d) { return getRectY(elt.sub, d) + self.options.cell.height / 2; })
						.attr('class', 'line');
					drawElts(elt.sub, newCol);
				}
			});

			var totalHeight = elts.length * (self.options.cell.height + self.options.cell.topPadding);
			var g = chart.selectAll('rect_' + col)
				.data(elts).enter()
				.append('g');
			g.append('foreignObject')
				// .attr('font-size', options.cell.fontSize)
				.attr('width', self.options.cell.width)
				.attr('height', self.options.cell.height)
				.attr('class', 'rect-text')
				.attr('x', function(d) { return  getRectX(col) + self.options.cell.leftTextPadding; })
				.attr('y', function(d)  { return getRectY(elts, d) + self.options.cell.topTextPadding; })
				.attr('text-anchor', 'middle')
				.html(function(d) {
					return d.title;
				});
			g.append('rect')
				.attr('width', self.options.cell.width)
				.attr('height', self.options.cell.height)
				.attr('rx', 20).attr('ry', 20)
				.attr('x', function(d) { return  getRectX(col); })
				.attr('y', function(d)  { return getRectY(elts, d); })
				.attr('class', function(d) {
					var result = 'rect-title';
					if (d.isSelected) {
						result += ' rect-title-selected';
					}
						return result;
				})
				.on("mouseover", function(d) {
					if (!d['info']) {
						return;
					}
					tooltipDiv.transition()
						.style("opacity", .9);
					tooltipDiv.html(d['info'])
						.style("left", (d3.event.pageX) + "px")
						.style("top", (d3.event.pageY - 28) + "px");
				})
				.on('mouseout', function(d) {
					if (!d['info']) {
						return;
					}

					tooltipDiv.transition()
						.style("opacity", 0);
					})
					.on('click', function(d) {
						if (!self.options.cell.click || self.options.cell.click == null) {
							return;
						}

						self.options.cell.click(d);
				});
		}

		function calculateNbColumns(elts) {
			var increment = false;
			elts.forEach(function(elt) {
				if (!increment) {
					nbCols++;
					increment = true;
				}

				if (elt.sub && elt.sub.length > 0) {
					calculateNbColumns(elt.sub);
				}
			});
		}

		var chart = d3.select(self.refs.main).append("svg").attr("height", self.options.frame.height);
		calculateNbColumns(self.options.data);
		if (self.options.frame.width == 'auto') {
			var width = nbCols * (self.options.cell.leftPadding + self.options.cell.width);
			chart.attr('width', width);
		} else {
			chart.attr('width', self.options.frame.with);
		}



		var tooltipDiv = d3.select("body").append("div")
			.attr("class", "tooltip")
			.style("opacity", 0);
		chart.append("defs").append("marker")
			.attr("id", "marker")
			.attr("viewBox", "-5 -5 10 10")
			.attr("refX", 15)
			.attr("refY", -1.5)
			.attr('markerUnits', 'strokeWidth')
			.attr("markerWidth", 5)
			.attr("markerHeight", 5)
			.attr('refX', 0)
			.attr('refY', 0)
			.attr("orient", "auto")
			.append("path").attr("d", "M 0,0 m -5,-5 L 5,0 L -5,5 Z");
		drawElts(self.options.data, 0);
	}
}

export default Workflow;