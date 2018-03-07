import React, { Component } from "react";

class Modal extends React.Component {
    constructor(props) {
        super(props);
        this.myDiv = null;
    }

    render() {
        return (
            <div className="modal fade" ref={c => this.myDiv = c}>
                <div className="modal-dialog modal-lg">
                    <div className="modal-content">
                        <div className="modal-header">
                            <h4 className="modal-title">{this.props.title}</h4>
                            <button type="button" className="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                        </div>
                        <div className="modal-body">
                            {this.props.children}
                        </div>
                        <div className="modal-footer">
                            <button type="button" className="btn btn-default" data-dismiss="modal">Fermer</button>
                            <button type="button" className="btn btn-default" onClick={this.props.onConfirmHandle}>{this.props.confirmTxt}</button>
                        </div>
                    </div>
                </div>
            </div>
        );
    }

    close() {
        $(this.myDiv).modal('toggle');
    }

    componentDidMount() {
        if (this.myDiv) {
            $(this.myDiv).modal('show');
            $(this.myDiv).on('hidden.bs.modal', this.props.handleHideModal);
        }
    }
}

export default Modal;