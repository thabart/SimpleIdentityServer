import React, { Component } from "react";
import $ from "jquery";
import 'jquery-ui/ui/widgets/dialog';

class Modal extends React.Component {
    constructor(props) {
        super(props);
        this.dialog = null;
        this.myDiv = null;
        this.state = {
            isVisible: false
        }
    }

    render() {
        var style = {};
        if (!this.state.isVisible) {
            style = {display: 'none'};
        }

        return (
            <div ref={c => this.myDiv = c} style={style} title={this.props.title}>
                {this.props.children}
            </div>
        );
    }

    close() {
        $(this.myDiv).modal('toggle');
    }

    show() {        
        var self = this;
        self.setState({
            isVisible: true
        });
        self.dialog = $(self.myDiv).dialog({
            resizable: false,
            height: "auto",
            width: 900,
            modal: true
        });
        self.dialog.on('dialogclose', function(e) {
            self.hide();
            if (self.props.onClose) {
                self.props.onClose();
            }
        });
    }

    hide() {
        this.setState({
            isVisible: false
        });
    }
}

export default Modal;