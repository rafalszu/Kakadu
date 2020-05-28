import React from "react";
import { connect } from "react-redux";
import { Reply } from "./Reply";

class Replies extends React.Component {
    render() {
        const { knownRouteReplies } = this.props;
        return (
            
                <ul className="list-group mb-3">
                    {knownRouteReplies && knownRouteReplies.map((reply, index) => 
                        <Reply 
                            key={reply.id}
                            reply={reply}
                            index={index+1} 
                        />
                    )}
                </ul>
            
        )
    }
}

function mapStateToProps(state) {
    const { knownRouteReplies } = state;
    return {
        knownRouteReplies
    }
}

const connectedKnownRouteRepliesPage = connect(mapStateToProps)(Replies);
export { connectedKnownRouteRepliesPage as Replies };
