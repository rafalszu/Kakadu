import React from "react";
import { connect } from "react-redux";
import { knownRouteActions } from '../../_actions'

class Reply extends React.Component {
    constructor(props) {
        super(props);

        this.handleReplySelected = this.handleReplySelected.bind(this);
    }
    
    handleReplySelected(reply) {
        this.props.dispatch(knownRouteActions.selectKnownRouteReply(reply));
    }

    render() {
        const { reply } = this.props;
        let isSelected = reply.id === (this.props.reply && this.props.reply.id)

        return (
            <li 
                key={reply.id} 
                style={{cursor: "pointer"}}
                className={`list-group-item d-flex justify-content-between lh-condensed ${isSelected ? "bg-light" : ""} `}
                onClick={() => this.handleReplySelected(reply)}   
            >
                <span>reply {this.props.index}</span>
            </li>
        )
    }
}

function mapStateToProps(state) {
    const { knownRouteReply } = state;
    return {
        knownRouteReply
    }
}

const connectedKnownRouteReplyPage = connect(mapStateToProps)(Reply);
export { connectedKnownRouteReplyPage as Reply };
